﻿using SpaceGame.Core.Components;
using SpaceGame.Infrastructure;
using System.Drawing;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using static SDL3.SDL;

namespace SpaceGame.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ColouredVertex
    {
        public Vector2 Vertex;
        public Vector4 Colour;
    }
    
    public class RenderSystem : IDisposable
    {
        public RenderSystem(IRenderer renderer)
        {
            _uploadBuffer = renderer.CreateTransferBuffer(sizeof(float) * 6 * 6 * 2, GPUTransferBufferUsage.Upload);
            _vertexBuffer = renderer.CreateVertexBuffer(sizeof(float) * 6 * 6 * 2);

            _renderer = renderer;
            var vertexShaderInfo = default(ShaderCreateInfo);
            vertexShaderInfo.Entrypoint = "main";
            vertexShaderInfo.Code = VertexShader;
            vertexShaderInfo.Stage = GPUShaderStage.Vertex;
            vertexShaderInfo.Format = GPUShaderFormat.SPIRV;
            using var vertexShader = renderer.CreateShader(ref vertexShaderInfo);

            var fragmentShaderInfo = default(ShaderCreateInfo);
            fragmentShaderInfo.Entrypoint = "main";
            fragmentShaderInfo.Code = FragmentShader;
            fragmentShaderInfo.Stage = GPUShaderStage.Fragment;
            fragmentShaderInfo.Format = GPUShaderFormat.SPIRV;
            using var fragmentShader = renderer.CreateShader(ref fragmentShaderInfo);

            var pipelineCreateInfo = new GraphicsPipelineCreateInfo(vertexShader, fragmentShader)
            {
                VertexInputState = new()
                {
                    VertexBufferDescriptions = [
                        new GPUVertexBufferDescription() { 
                            Slot = 0,
                            Pitch = sizeof(float) * 6,
                            InputRate = GPUVertexInputRate.Vertex,
                            InstanceStepRate = 0
                        }],
                    VertexAttributes = [
                        new GPUVertexAttribute()
                        {
                            Location = 0,
                            BufferSlot = 0,
                            Format = GPUVertexElementFormat.Float2,
                            Offset = 0
                        },
                        new GPUVertexAttribute()
                        {
                            Location = 1,
                            BufferSlot = 0,
                            Format = GPUVertexElementFormat.Float4,
                            Offset = sizeof(float) * 2
                        },
                    ]
                },
                PrimitiveType = GPUPrimitiveType.TriangleList,
                TargetInfo = new()
                {
                    ColorTargetDescriptions = [
                        new() 
                        {
                            Format=GetGPUSwapchainTextureFormat(renderer.GpuDevice.Handle, renderer.Window.Handle),
                        }
                    ]
                }
            };
            _pipeline = renderer.CreatePipeline(ref pipelineCreateInfo);
        }

        private readonly IRenderer _renderer;
        private readonly ITransferBuffer _uploadBuffer;
        private readonly IVertexBuffer _vertexBuffer;
        private readonly IGraphicsPipeline _pipeline;
        private readonly List<Tuple<Transform, Sprite>> _sprites = [];

        private unsafe void UploadVertices(nint commandBuffer, nint copyPass)
        {
            var vertexSize = sizeof(float) * 6 * 6 * _sprites.Count;
            if (!_vertexBuffer.TryResize(vertexSize) || !_uploadBuffer.TryResize(vertexSize))
            {
                return;
            }

            var mappedBufferPtr = MapGPUTransferBuffer(_renderer.GpuDevice.Handle, _uploadBuffer.Handle, false);
            if (mappedBufferPtr == nint.Zero)
            {
                return;
            }
            var mappedBuffer = (ColouredVertex*)mappedBufferPtr;

            //int screenWidth = GetWindow
            int bufferIndex = 0;
            foreach (var entity in _sprites)
            {
                var transform = entity.Item1;
                var sprite = entity.Item2;
                
                mappedBuffer[bufferIndex].Vertex = transform.Position;
                mappedBuffer[bufferIndex++].Colour = new Vector4(0f, 1f, 0f, 1f);
                
                mappedBuffer[bufferIndex].Vertex = transform.Position + sprite.Size;
                mappedBuffer[bufferIndex++].Colour = new Vector4(0f, 1f, 0f, 1f);
                
                mappedBuffer[bufferIndex].Vertex = transform.Position with { X = transform.Position.X + sprite.Size.X };
                mappedBuffer[bufferIndex++].Colour = new Vector4(0f, 1f, 0f, 1f);
                
                mappedBuffer[bufferIndex].Vertex = transform.Position;
                mappedBuffer[bufferIndex++].Colour = new Vector4(0f, 1f, 0f, 1f);
                
                mappedBuffer[bufferIndex].Vertex = transform.Position with { Y = transform.Position.Y + sprite.Size.Y };
                mappedBuffer[bufferIndex++].Colour = new Vector4(0f, 1f, 0f, 1f);
                
                mappedBuffer[bufferIndex].Vertex = transform.Position + sprite.Size;
                mappedBuffer[bufferIndex++].Colour = new Vector4(0f, 1f, 0f, 1f);
            }

            UnmapGPUTransferBuffer(_renderer.GpuDevice.Handle, _uploadBuffer.Handle);

            GPUTransferBufferLocation source = new()
            {
                TransferBuffer = _uploadBuffer.Handle,
                Offset = 0,
            };
            GPUBufferRegion destination = new()
            {
                Buffer = _vertexBuffer.Handle,
                Offset = 0,
                Size = (uint)(sizeof(float) * 6 * 6 * _sprites.Count)
            };
            UploadToGPUBuffer(copyPass, source, destination, false);
        }

        public void Add(Transform transform, Sprite sprite)
        {
            _sprites.Add(Tuple.Create(transform, sprite));
        }

        public void Clear()
        {
            _sprites.Clear();
        }

        public void Draw()
        {
            _renderer
                .AcquireCommandBuffer()
                .WithCopyPass((cmd, pass) =>
                {
                    UploadVertices(cmd, pass);
                })
                .AcquireSwapchainTexture()
                .Update(cmd =>
                {
                    cmd.ColorTargetInfo = [
                        new ()
                        {
                            Texture = cmd.SwapchainTexture,
                            ClearColor = new FColor { R = 0.5f, G = 0, B = 0.5f, A = 1.0f },
                            LoadOp = GPULoadOp.Clear,
                            StoreOp = GPUStoreOp.Store
                        }
                    ];
                })
                .WithRenderPass(
                    (cmd, pass) =>
                    {
                        var bufferBinding = new[] {
                            new GPUBufferBinding
                            {
                                Buffer = _vertexBuffer.Handle,
                                Offset = 0
                            }
                        };

                        BindGPUGraphicsPipeline(pass, _pipeline.Handle);
                        BindGPUVertexBuffers(pass, 0, bufferBinding, (uint)bufferBinding.Length);
                        DrawGPUPrimitives(pass, (uint)(6 * _sprites.Count), (uint)_sprites.Count, 0, 0);
                    })
                .Submit();
        }

        public void Dispose()
        {
            _pipeline?.Dispose();
            _vertexBuffer?.Dispose();
            _uploadBuffer?.Dispose();
            GC.SuppressFinalize(this);
        }

        private static byte[] VertexShader => [0x3, 0x2, 0x23, 0x7, 0x0, 0x5, 0x1, 0x0, 0x0, 0x0, 0x28, 0x0, 0x82, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x11, 0x0, 0x2, 0x0, 0x1, 0x0, 0x0, 0x0, 0xa, 0x0, 0x8, 0x0, 0x53, 0x50, 0x56, 0x5f, 0x4b, 0x48, 0x52, 0x5f, 0x6e, 0x6f, 0x6e, 0x5f, 0x73, 0x65, 0x6d, 0x61, 0x6e, 0x74, 0x69, 0x63, 0x5f, 0x69, 0x6e, 0x66, 0x6f, 0x0, 0x0, 0x0, 0xb, 0x0, 0xb, 0x0, 0x2, 0x0, 0x0, 0x0, 0x4e, 0x6f, 0x6e, 0x53, 0x65, 0x6d, 0x61, 0x6e, 0x74, 0x69, 0x63, 0x2e, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x2e, 0x44, 0x65, 0x62, 0x75, 0x67, 0x49, 0x6e, 0x66, 0x6f, 0x2e, 0x31, 0x30, 0x30, 0x0, 0x0, 0x0, 0x0, 0xe, 0x0, 0x3, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0xf, 0x0, 0x9, 0x0, 0x0, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x6d, 0x61, 0x69, 0x6e, 0x0, 0x0, 0x0, 0x0, 0x59, 0x0, 0x0, 0x0, 0x5c, 0x0, 0x0, 0x0, 0x22, 0x0, 0x0, 0x0, 0x25, 0x0, 0x0, 0x0, 0x7, 0x0, 0xb2, 0x0, 0x1, 0x0, 0x0, 0x0, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x41, 0x73, 0x73, 0x65, 0x6d, 0x62, 0x6c, 0x65, 0x64, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x32, 0x20, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x3a, 0x20, 0x50, 0x4f, 0x53, 0x49, 0x54, 0x49, 0x4f, 0x4e, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x3a, 0x20, 0x53, 0x56, 0x5f, 0x50, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x5b, 0x73, 0x68, 0x61, 0x64, 0x65, 0x72, 0x28, 0x22, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x22, 0x29, 0x5d, 0xd, 0xa, 0x66, 0x75, 0x6e, 0x63, 0x20, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4d, 0x61, 0x69, 0x6e, 0x28, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x3a, 0x20, 0x41, 0x73, 0x73, 0x65, 0x6d, 0x62, 0x6c, 0x65, 0x64, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x29, 0x20, 0x2d, 0x3e, 0x20, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x2e, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x3d, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x28, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x2e, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x2c, 0x20, 0x30, 0x2e, 0x30, 0x66, 0x2c, 0x20, 0x31, 0x2e, 0x30, 0x66, 0x29, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3d, 0x20, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x72, 0x65, 0x74, 0x75, 0x72, 0x6e, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x7d, 0xd, 0xa, 0xd, 0xa, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x49, 0x6e, 0x70, 0x75, 0x74, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x5b, 0x73, 0x68, 0x61, 0x64, 0x65, 0x72, 0x28, 0x22, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x22, 0x29, 0x5d, 0xd, 0xa, 0x66, 0x75, 0x6e, 0x63, 0x20, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4d, 0x61, 0x69, 0x6e, 0x28, 0x69, 0x6e, 0x70, 0x75, 0x74, 0x3a, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x49, 0x6e, 0x70, 0x75, 0x74, 0x29, 0x20, 0x2d, 0x3e, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3d, 0x20, 0x69, 0x6e, 0x70, 0x75, 0x74, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x72, 0x65, 0x74, 0x75, 0x72, 0x6e, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x7d, 0xd, 0xa, 0x0, 0x0, 0x0, 0x7, 0x0, 0xf, 0x0, 0x5, 0x0, 0x0, 0x0, 0x43, 0x3a, 0x5c, 0x72, 0x65, 0x70, 0x6f, 0x73, 0x5c, 0x53, 0x70, 0x61, 0x63, 0x65, 0x47, 0x61, 0x6d, 0x65, 0x5c, 0x53, 0x70, 0x61, 0x63, 0x65, 0x47, 0x61, 0x6d, 0x65, 0x2e, 0x41, 0x73, 0x73, 0x65, 0x74, 0x73, 0x5c, 0x74, 0x72, 0x69, 0x61, 0x6e, 0x67, 0x6c, 0x65, 0x2e, 0x73, 0x6c, 0x61, 0x6e, 0x67, 0x0, 0x0, 0x3, 0x0, 0x3, 0x0, 0xb, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x7, 0x0, 0x5, 0x0, 0x17, 0x0, 0x0, 0x0, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4d, 0x61, 0x69, 0x6e, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x1d, 0x0, 0x0, 0x0, 0x73, 0x6c, 0x61, 0x6e, 0x67, 0x63, 0x0, 0x0, 0x7, 0x0, 0x29, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x2d, 0x74, 0x61, 0x72, 0x67, 0x65, 0x74, 0x20, 0x73, 0x70, 0x69, 0x72, 0x76, 0x20, 0x20, 0x2d, 0x49, 0x20, 0x22, 0x43, 0x3a, 0x5c, 0x55, 0x73, 0x65, 0x72, 0x73, 0x5c, 0x36, 0x31, 0x30, 0x34, 0x35, 0x32, 0x32, 0x5c, 0x73, 0x63, 0x6f, 0x6f, 0x70, 0x5c, 0x61, 0x70, 0x70, 0x73, 0x5c, 0x76, 0x75, 0x6c, 0x6b, 0x61, 0x6e, 0x5c, 0x63, 0x75, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x5c, 0x42, 0x69, 0x6e, 0x22, 0x20, 0x2d, 0x6d, 0x61, 0x74, 0x72, 0x69, 0x78, 0x2d, 0x6c, 0x61, 0x79, 0x6f, 0x75, 0x74, 0x2d, 0x63, 0x6f, 0x6c, 0x75, 0x6d, 0x6e, 0x2d, 0x6d, 0x61, 0x6a, 0x6f, 0x72, 0x20, 0x2d, 0x63, 0x61, 0x70, 0x61, 0x62, 0x69, 0x6c, 0x69, 0x74, 0x79, 0x20, 0x73, 0x70, 0x69, 0x72, 0x76, 0x5f, 0x31, 0x5f, 0x30, 0x20, 0x2d, 0x73, 0x74, 0x61, 0x67, 0x65, 0x20, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x20, 0x2d, 0x65, 0x6e, 0x74, 0x72, 0x79, 0x20, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4d, 0x61, 0x69, 0x6e, 0x20, 0x2d, 0x67, 0x32, 0x0, 0x0, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x2b, 0x0, 0x0, 0x0, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x0, 0x0, 0x0, 0x7, 0x0, 0x5, 0x0, 0x32, 0x0, 0x0, 0x0, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x0, 0x0, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x37, 0x0, 0x0, 0x0, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x0, 0x0, 0x0, 0x7, 0x0, 0x6, 0x0, 0x39, 0x0, 0x0, 0x0, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x0, 0x0, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x3d, 0x0, 0x0, 0x0, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x0, 0x0, 0x5, 0x0, 0x6, 0x0, 0x22, 0x0, 0x0, 0x0, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x2e, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x0, 0x5, 0x0, 0x6, 0x0, 0x25, 0x0, 0x0, 0x0, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x0, 0x0, 0x0, 0x0, 0x5, 0x0, 0x4, 0x0, 0x3c, 0x0, 0x0, 0x0, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x0, 0x0, 0x5, 0x0, 0xb, 0x0, 0x5c, 0x0, 0x0, 0x0, 0x65, 0x6e, 0x74, 0x72, 0x79, 0x50, 0x6f, 0x69, 0x6e, 0x74, 0x50, 0x61, 0x72, 0x61, 0x6d, 0x5f, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4d, 0x61, 0x69, 0x6e, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x0, 0x0, 0x0, 0x0, 0x5, 0x0, 0x5, 0x0, 0xb, 0x0, 0x0, 0x0, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4d, 0x61, 0x69, 0x6e, 0x0, 0x0, 0x47, 0x0, 0x4, 0x0, 0x22, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x47, 0x0, 0x4, 0x0, 0x25, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x47, 0x0, 0x4, 0x0, 0x59, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x47, 0x0, 0x4, 0x0, 0x5c, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x13, 0x0, 0x2, 0x0, 0x3, 0x0, 0x0, 0x0, 0x15, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x5, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x9, 0x0, 0x0, 0x0, 0x64, 0x0, 0x0, 0x0, 0x21, 0x0, 0x3, 0x0, 0xc, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0x16, 0x0, 0x3, 0x0, 0xf, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x17, 0x0, 0x4, 0x0, 0x10, 0x0, 0x0, 0x0, 0xf, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0xe, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0x6, 0x0, 0x0, 0x0, 0x17, 0x0, 0x4, 0x0, 0x1f, 0x0, 0x0, 0x0, 0xf, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x20, 0x0, 0x4, 0x0, 0x21, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x1f, 0x0, 0x0, 0x0, 0x20, 0x0, 0x4, 0x0, 0x24, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x27, 0x0, 0x0, 0x0, 0xf, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x2c, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x2d, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x2e, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x30, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x33, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x34, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x35, 0x0, 0x0, 0x0, 0x80, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x3a, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x0, 0x0, 0x12, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x42, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x15, 0x0, 0x4, 0x0, 0x43, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x43, 0x0, 0x0, 0x0, 0x44, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20, 0x0, 0x4, 0x0, 0x45, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0xf, 0x0, 0x0, 0x0, 0x48, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0xf, 0x0, 0x0, 0x0, 0x49, 0x0, 0x0, 0x0, 0x0, 0x0, 0x80, 0x3f, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x4e, 0x0, 0x0, 0x0, 0x11, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x43, 0x0, 0x0, 0x0, 0x4f, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x55, 0x0, 0x0, 0x0, 0x13, 0x0, 0x0, 0x0, 0x20, 0x0, 0x4, 0x0, 0x58, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x21, 0x0, 0x0, 0x0, 0x22, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x24, 0x0, 0x0, 0x0, 0x25, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x58, 0x0, 0x0, 0x0, 0x59, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x58, 0x0, 0x0, 0x0, 0x5c, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x72, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x5, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x9, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x14, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0xc, 0x0, 0xe, 0x0, 0x3, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x14, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x14, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0x1c, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x6b, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x1d, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0x2a, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x0, 0x0, 0x2c, 0x0, 0x0, 0x0, 0x2d, 0x0, 0x0, 0x0, 0x2e, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x2f, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x6, 0x0, 0x0, 0x0, 0x2a, 0x0, 0x0, 0x0, 0x30, 0x0, 0x0, 0x0, 0xc, 0x0, 0xd, 0x0, 0x3, 0x0, 0x0, 0x0, 0x31, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x32, 0x0, 0x0, 0x0, 0x2f, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x33, 0x0, 0x0, 0x0, 0x34, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x35, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0xc, 0x0, 0xd, 0x0, 0x3, 0x0, 0x0, 0x0, 0x36, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x37, 0x0, 0x0, 0x0, 0x2f, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x33, 0x0, 0x0, 0x0, 0x34, 0x0, 0x0, 0x0, 0x35, 0x0, 0x0, 0x0, 0x35, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0xc, 0x0, 0x10, 0x0, 0x3, 0x0, 0x0, 0x0, 0x38, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x39, 0x0, 0x0, 0x0, 0x3a, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x33, 0x0, 0x0, 0x0, 0x34, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x39, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x0, 0x0, 0x2e, 0x0, 0x0, 0x0, 0x31, 0x0, 0x0, 0x0, 0x36, 0x0, 0x0, 0x0, 0xc, 0x0, 0xc, 0x0, 0x3, 0x0, 0x0, 0x0, 0x3c, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1a, 0x0, 0x0, 0x0, 0x3d, 0x0, 0x0, 0x0, 0x38, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x27, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x71, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1f, 0x0, 0x0, 0x0, 0x72, 0x0, 0x0, 0x0, 0x36, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc, 0x0, 0x0, 0x0, 0xf8, 0x0, 0x2, 0x0, 0xd, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x45, 0x0, 0x0, 0x0, 0x70, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x45, 0x0, 0x0, 0x0, 0x6f, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x7d, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0xc, 0x0, 0xa, 0x0, 0x3, 0x0, 0x0, 0x0, 0x7c, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x27, 0x0, 0x0, 0x0, 0x27, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0x7b, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1d, 0x0, 0x0, 0x0, 0x3c, 0x0, 0x0, 0x0, 0x70, 0x0, 0x0, 0x0, 0x71, 0x0, 0x0, 0x0, 0x4f, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0x76, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1d, 0x0, 0x0, 0x0, 0x3c, 0x0, 0x0, 0x0, 0x6f, 0x0, 0x0, 0x0, 0x71, 0x0, 0x0, 0x0, 0x44, 0x0, 0x0, 0x0, 0xc, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0x7e, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x68, 0x0, 0x0, 0x0, 0xc, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0x7f, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x1a, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x65, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x80, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0x3d, 0x0, 0x4, 0x0, 0x1f, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x22, 0x0, 0x0, 0x0, 0x3d, 0x0, 0x4, 0x0, 0x10, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x25, 0x0, 0x0, 0x0, 0xc, 0x0, 0xa, 0x0, 0x3, 0x0, 0x0, 0x0, 0x60, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x42, 0x0, 0x0, 0x0, 0x42, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0x50, 0x0, 0x6, 0x0, 0x10, 0x0, 0x0, 0x0, 0x47, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x48, 0x0, 0x0, 0x0, 0x49, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x3, 0x0, 0x6f, 0x0, 0x0, 0x0, 0x47, 0x0, 0x0, 0x0, 0xc, 0x0, 0xa, 0x0, 0x3, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x4e, 0x0, 0x0, 0x0, 0x4e, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x3, 0x0, 0x70, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0xc, 0x0, 0xa, 0x0, 0x3, 0x0, 0x0, 0x0, 0x6a, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x55, 0x0, 0x0, 0x0, 0x55, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x3, 0x0, 0x59, 0x0, 0x0, 0x0, 0x47, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x3, 0x0, 0x5c, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0xfd, 0x0, 0x1, 0x0, 0xc, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0x81, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0x38, 0x0, 0x1, 0x0];
        private static byte[] FragmentShader => [0x3, 0x2, 0x23, 0x7, 0x0, 0x5, 0x1, 0x0, 0x0, 0x0, 0x28, 0x0, 0x61, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x11, 0x0, 0x2, 0x0, 0x1, 0x0, 0x0, 0x0, 0xa, 0x0, 0x8, 0x0, 0x53, 0x50, 0x56, 0x5f, 0x4b, 0x48, 0x52, 0x5f, 0x6e, 0x6f, 0x6e, 0x5f, 0x73, 0x65, 0x6d, 0x61, 0x6e, 0x74, 0x69, 0x63, 0x5f, 0x69, 0x6e, 0x66, 0x6f, 0x0, 0x0, 0x0, 0xb, 0x0, 0xb, 0x0, 0x2, 0x0, 0x0, 0x0, 0x4e, 0x6f, 0x6e, 0x53, 0x65, 0x6d, 0x61, 0x6e, 0x74, 0x69, 0x63, 0x2e, 0x53, 0x68, 0x61, 0x64, 0x65, 0x72, 0x2e, 0x44, 0x65, 0x62, 0x75, 0x67, 0x49, 0x6e, 0x66, 0x6f, 0x2e, 0x31, 0x30, 0x30, 0x0, 0x0, 0x0, 0x0, 0xe, 0x0, 0x3, 0x0, 0x0, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0xf, 0x0, 0x7, 0x0, 0x4, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x6d, 0x61, 0x69, 0x6e, 0x0, 0x0, 0x0, 0x0, 0x48, 0x0, 0x0, 0x0, 0x21, 0x0, 0x0, 0x0, 0x10, 0x0, 0x3, 0x0, 0xb, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0x7, 0x0, 0xb2, 0x0, 0x1, 0x0, 0x0, 0x0, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x41, 0x73, 0x73, 0x65, 0x6d, 0x62, 0x6c, 0x65, 0x64, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x32, 0x20, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x3a, 0x20, 0x50, 0x4f, 0x53, 0x49, 0x54, 0x49, 0x4f, 0x4e, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x3a, 0x20, 0x53, 0x56, 0x5f, 0x50, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x5b, 0x73, 0x68, 0x61, 0x64, 0x65, 0x72, 0x28, 0x22, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x22, 0x29, 0x5d, 0xd, 0xa, 0x66, 0x75, 0x6e, 0x63, 0x20, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4d, 0x61, 0x69, 0x6e, 0x28, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x3a, 0x20, 0x41, 0x73, 0x73, 0x65, 0x6d, 0x62, 0x6c, 0x65, 0x64, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x29, 0x20, 0x2d, 0x3e, 0x20, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x56, 0x65, 0x72, 0x74, 0x65, 0x78, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x2e, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x20, 0x3d, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x28, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x2e, 0x70, 0x6f, 0x73, 0x69, 0x74, 0x69, 0x6f, 0x6e, 0x2c, 0x20, 0x30, 0x2e, 0x30, 0x66, 0x2c, 0x20, 0x31, 0x2e, 0x30, 0x66, 0x29, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3d, 0x20, 0x76, 0x65, 0x72, 0x74, 0x65, 0x78, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x72, 0x65, 0x74, 0x75, 0x72, 0x6e, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x7d, 0xd, 0xa, 0xd, 0xa, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x49, 0x6e, 0x70, 0x75, 0x74, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x73, 0x74, 0x72, 0x75, 0x63, 0x74, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0xd, 0xa, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x34, 0x20, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3a, 0x20, 0x43, 0x4f, 0x4c, 0x4f, 0x52, 0x3b, 0xd, 0xa, 0x7d, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x5b, 0x73, 0x68, 0x61, 0x64, 0x65, 0x72, 0x28, 0x22, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x22, 0x29, 0x5d, 0xd, 0xa, 0x66, 0x75, 0x6e, 0x63, 0x20, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4d, 0x61, 0x69, 0x6e, 0x28, 0x69, 0x6e, 0x70, 0x75, 0x74, 0x3a, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x49, 0x6e, 0x70, 0x75, 0x74, 0x29, 0x20, 0x2d, 0x3e, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x7b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x20, 0x3d, 0x20, 0x69, 0x6e, 0x70, 0x75, 0x74, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x3b, 0xd, 0xa, 0xd, 0xa, 0x20, 0x20, 0x20, 0x20, 0x72, 0x65, 0x74, 0x75, 0x72, 0x6e, 0x20, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x3b, 0xd, 0xa, 0x7d, 0xd, 0xa, 0x0, 0x0, 0x0, 0x7, 0x0, 0xf, 0x0, 0x5, 0x0, 0x0, 0x0, 0x43, 0x3a, 0x5c, 0x72, 0x65, 0x70, 0x6f, 0x73, 0x5c, 0x53, 0x70, 0x61, 0x63, 0x65, 0x47, 0x61, 0x6d, 0x65, 0x5c, 0x53, 0x70, 0x61, 0x63, 0x65, 0x47, 0x61, 0x6d, 0x65, 0x2e, 0x41, 0x73, 0x73, 0x65, 0x74, 0x73, 0x5c, 0x74, 0x72, 0x69, 0x61, 0x6e, 0x67, 0x6c, 0x65, 0x2e, 0x73, 0x6c, 0x61, 0x6e, 0x67, 0x0, 0x0, 0x3, 0x0, 0x3, 0x0, 0xb, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x7, 0x0, 0x6, 0x0, 0x17, 0x0, 0x0, 0x0, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4d, 0x61, 0x69, 0x6e, 0x0, 0x0, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x1d, 0x0, 0x0, 0x0, 0x73, 0x6c, 0x61, 0x6e, 0x67, 0x63, 0x0, 0x0, 0x7, 0x0, 0x29, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x2d, 0x74, 0x61, 0x72, 0x67, 0x65, 0x74, 0x20, 0x73, 0x70, 0x69, 0x72, 0x76, 0x20, 0x20, 0x2d, 0x49, 0x20, 0x22, 0x43, 0x3a, 0x5c, 0x55, 0x73, 0x65, 0x72, 0x73, 0x5c, 0x36, 0x31, 0x30, 0x34, 0x35, 0x32, 0x32, 0x5c, 0x73, 0x63, 0x6f, 0x6f, 0x70, 0x5c, 0x61, 0x70, 0x70, 0x73, 0x5c, 0x76, 0x75, 0x6c, 0x6b, 0x61, 0x6e, 0x5c, 0x63, 0x75, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x5c, 0x42, 0x69, 0x6e, 0x22, 0x20, 0x2d, 0x6d, 0x61, 0x74, 0x72, 0x69, 0x78, 0x2d, 0x6c, 0x61, 0x79, 0x6f, 0x75, 0x74, 0x2d, 0x63, 0x6f, 0x6c, 0x75, 0x6d, 0x6e, 0x2d, 0x6d, 0x61, 0x6a, 0x6f, 0x72, 0x20, 0x2d, 0x63, 0x61, 0x70, 0x61, 0x62, 0x69, 0x6c, 0x69, 0x74, 0x79, 0x20, 0x73, 0x70, 0x69, 0x72, 0x76, 0x5f, 0x31, 0x5f, 0x30, 0x20, 0x2d, 0x73, 0x74, 0x61, 0x67, 0x65, 0x20, 0x70, 0x69, 0x78, 0x65, 0x6c, 0x20, 0x2d, 0x65, 0x6e, 0x74, 0x72, 0x79, 0x20, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4d, 0x61, 0x69, 0x6e, 0x20, 0x2d, 0x67, 0x32, 0x0, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x27, 0x0, 0x0, 0x0, 0x66, 0x6c, 0x6f, 0x61, 0x74, 0x0, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x2e, 0x0, 0x0, 0x0, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x0, 0x0, 0x0, 0x7, 0x0, 0x6, 0x0, 0x33, 0x0, 0x0, 0x0, 0x46, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x0, 0x0, 0x7, 0x0, 0x4, 0x0, 0x36, 0x0, 0x0, 0x0, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x0, 0x0, 0x5, 0x0, 0x5, 0x0, 0x21, 0x0, 0x0, 0x0, 0x69, 0x6e, 0x70, 0x75, 0x74, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x0, 0x5, 0x0, 0x4, 0x0, 0x35, 0x0, 0x0, 0x0, 0x6f, 0x75, 0x74, 0x70, 0x75, 0x74, 0x0, 0x0, 0x5, 0x0, 0xb, 0x0, 0x48, 0x0, 0x0, 0x0, 0x65, 0x6e, 0x74, 0x72, 0x79, 0x50, 0x6f, 0x69, 0x6e, 0x74, 0x50, 0x61, 0x72, 0x61, 0x6d, 0x5f, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4d, 0x61, 0x69, 0x6e, 0x2e, 0x63, 0x6f, 0x6c, 0x6f, 0x72, 0x0, 0x0, 0x5, 0x0, 0x6, 0x0, 0xb, 0x0, 0x0, 0x0, 0x66, 0x72, 0x61, 0x67, 0x6d, 0x65, 0x6e, 0x74, 0x4d, 0x61, 0x69, 0x6e, 0x0, 0x0, 0x0, 0x0, 0x47, 0x0, 0x4, 0x0, 0x21, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x47, 0x0, 0x4, 0x0, 0x48, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x13, 0x0, 0x2, 0x0, 0x3, 0x0, 0x0, 0x0, 0x15, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x5, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x9, 0x0, 0x0, 0x0, 0x64, 0x0, 0x0, 0x0, 0x21, 0x0, 0x3, 0x0, 0xc, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0x16, 0x0, 0x3, 0x0, 0xf, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x17, 0x0, 0x4, 0x0, 0x10, 0x0, 0x0, 0x0, 0xf, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0x21, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0x6, 0x0, 0x0, 0x0, 0x20, 0x0, 0x4, 0x0, 0x20, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x22, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x28, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x29, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x2a, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x2c, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x2f, 0x0, 0x0, 0x0, 0x1b, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x30, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x31, 0x0, 0x0, 0x0, 0x80, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x34, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x37, 0x0, 0x0, 0x0, 0x14, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x15, 0x0, 0x4, 0x0, 0x3c, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x3c, 0x0, 0x0, 0x0, 0x3d, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x20, 0x0, 0x4, 0x0, 0x3e, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x4, 0x0, 0x6, 0x0, 0x0, 0x0, 0x44, 0x0, 0x0, 0x0, 0x25, 0x0, 0x0, 0x0, 0x20, 0x0, 0x4, 0x0, 0x47, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0x10, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x20, 0x0, 0x0, 0x0, 0x21, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x47, 0x0, 0x0, 0x0, 0x48, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x56, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x5, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1, 0x0, 0x0, 0x0, 0x9, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x14, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x3, 0x0, 0x0, 0x0, 0xc, 0x0, 0xe, 0x0, 0x3, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x14, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x14, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0x1c, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x6b, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x1d, 0x0, 0x0, 0x0, 0x1e, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0x26, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x27, 0x0, 0x0, 0x0, 0x28, 0x0, 0x0, 0x0, 0x29, 0x0, 0x0, 0x0, 0x2a, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x6, 0x0, 0x0, 0x0, 0x26, 0x0, 0x0, 0x0, 0x2c, 0x0, 0x0, 0x0, 0xc, 0x0, 0xd, 0x0, 0x3, 0x0, 0x0, 0x0, 0x2d, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x2e, 0x0, 0x0, 0x0, 0x2b, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2f, 0x0, 0x0, 0x0, 0x30, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0x31, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0xc, 0x0, 0xf, 0x0, 0x3, 0x0, 0x0, 0x0, 0x32, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x33, 0x0, 0x0, 0x0, 0x34, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x2f, 0x0, 0x0, 0x0, 0x30, 0x0, 0x0, 0x0, 0xa, 0x0, 0x0, 0x0, 0x33, 0x0, 0x0, 0x0, 0x31, 0x0, 0x0, 0x0, 0x2a, 0x0, 0x0, 0x0, 0x2d, 0x0, 0x0, 0x0, 0xc, 0x0, 0xc, 0x0, 0x3, 0x0, 0x0, 0x0, 0x35, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1a, 0x0, 0x0, 0x0, 0x36, 0x0, 0x0, 0x0, 0x32, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x37, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0x15, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x55, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1f, 0x0, 0x0, 0x0, 0x56, 0x0, 0x0, 0x0, 0x36, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0xc, 0x0, 0x0, 0x0, 0xf8, 0x0, 0x2, 0x0, 0xd, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x4, 0x0, 0x3e, 0x0, 0x0, 0x0, 0x54, 0x0, 0x0, 0x0, 0x7, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x5c, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0xc, 0x0, 0xa, 0x0, 0x3, 0x0, 0x0, 0x0, 0x5b, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x23, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0xc, 0x0, 0x9, 0x0, 0x3, 0x0, 0x0, 0x0, 0x5a, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x1d, 0x0, 0x0, 0x0, 0x35, 0x0, 0x0, 0x0, 0x54, 0x0, 0x0, 0x0, 0x55, 0x0, 0x0, 0x0, 0x3d, 0x0, 0x0, 0x0, 0xc, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0x5d, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x68, 0x0, 0x0, 0x0, 0xc, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0x5e, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0xc, 0x0, 0x7, 0x0, 0x3, 0x0, 0x0, 0x0, 0x1a, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x65, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0xb, 0x0, 0x0, 0x0, 0xc, 0x0, 0x6, 0x0, 0x3, 0x0, 0x0, 0x0, 0x5f, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x17, 0x0, 0x0, 0x0, 0x16, 0x0, 0x0, 0x0, 0x3d, 0x0, 0x4, 0x0, 0x10, 0x0, 0x0, 0x0, 0x1f, 0x0, 0x0, 0x0, 0x21, 0x0, 0x0, 0x0, 0xc, 0x0, 0xa, 0x0, 0x3, 0x0, 0x0, 0x0, 0x4e, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x0, 0x0, 0x3b, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x3, 0x0, 0x54, 0x0, 0x0, 0x0, 0x1f, 0x0, 0x0, 0x0, 0xc, 0x0, 0xa, 0x0, 0x3, 0x0, 0x0, 0x0, 0x51, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x67, 0x0, 0x0, 0x0, 0x4, 0x0, 0x0, 0x0, 0x44, 0x0, 0x0, 0x0, 0x44, 0x0, 0x0, 0x0, 0x8, 0x0, 0x0, 0x0, 0x19, 0x0, 0x0, 0x0, 0x3e, 0x0, 0x3, 0x0, 0x48, 0x0, 0x0, 0x0, 0x1f, 0x0, 0x0, 0x0, 0xfd, 0x0, 0x1, 0x0, 0xc, 0x0, 0x5, 0x0, 0x3, 0x0, 0x0, 0x0, 0x60, 0x0, 0x0, 0x0, 0x2, 0x0, 0x0, 0x0, 0x18, 0x0, 0x0, 0x0, 0x38, 0x0, 0x1, 0x0];
    }
}
