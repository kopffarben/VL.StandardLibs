﻿using ImGuiNET;
using Stride.Graphics;
using Stride.Core.Mathematics;
using Buffer = Stride.Graphics.Buffer;
using Stride.Rendering;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VL.Skia;
using Stride.Core;
using Stride.Input;
using VL.Stride.Input;

namespace VL.ImGui
{
    partial class ImGuiRenderer
    {
        void CheckBuffers(ImDrawDataPtr drawData)
        {
            uint totalVBOSize = (uint)(drawData.TotalVtxCount * Unsafe.SizeOf<ImDrawVert>());
            if (totalVBOSize > vertexBinding.Buffer.SizeInBytes)
            {
                var vertexBuffer = Buffer.Vertex.New(device, (int)(totalVBOSize * 1.5f));
                vertexBinding = new VertexBufferBinding(vertexBuffer, imVertLayout, 0);
            }

            uint totalIBOSize = (uint)(drawData.TotalIdxCount * sizeof(ushort));
            if (totalIBOSize > indexBinding?.Buffer.SizeInBytes)
            {
                var is32Bits = false;
                var indexBuffer = Buffer.Index.New(device, (int)(totalIBOSize * 1.5f));
                indexBinding = new IndexBufferBinding(indexBuffer, is32Bits, 0);
            }
        }

        void UpdateBuffers(ImDrawDataPtr drawData)
        {
            // copy de dators
            int vtxOffsetBytes = 0;
            int idxOffsetBytes = 0;

            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];
                vertexBinding.Buffer.SetData(commandList, new DataPointer(cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>()), vtxOffsetBytes);
                indexBinding.Buffer.SetData(commandList, new DataPointer(cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size * sizeof(ushort)), idxOffsetBytes);
                vtxOffsetBytes += cmdList.VtxBuffer.Size * Unsafe.SizeOf<ImDrawVert>();
                idxOffsetBytes += cmdList.IdxBuffer.Size * sizeof(ushort);
            }
        }

        void RenderDrawLists(RenderDrawContext context, ImDrawDataPtr drawData)
        {
            CheckBuffers(drawData); // potentially resize buffers first if needed
            UpdateBuffers(drawData); // updeet em now

            int vtxOffset = 0;
            int idxOffset = 0;
            for (int n = 0; n < drawData.CmdListsCount; n++)
            {
                ImDrawListPtr cmdList = drawData.CmdLists[n];

                for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
                {
                    ImDrawCmdPtr cmd = cmdList.CmdBuffer[i];

                    if (cmd.UserCallback != IntPtr.Zero)
                    {
                        renderLayerHandle = GCHandle.FromIntPtr(cmd.UserCallback);
                        if (renderLayerHandle.Target is RenderLayer renderLayer)
                        {
                            if (renderLayer?.Viewport != null)
                            {
                                var renderContext = context?.RenderContext;
                                using (renderContext.PushTagAndRestore(InputExtensions.WindowInputSource, lastInputSource))
                                using (renderContext?.SaveRenderOutputAndRestore())
                                using (renderContext?.SaveViewportAndRestore())
                                {
                                    context?.CommandList.SetViewport((Viewport)renderLayer.Viewport);
                                    renderLayer.Layer?.Draw(context);
                                    context?.CommandList.SetViewport(renderContext.ViewportState.Viewport0);
                                }
                            }
                        }
                        else if (renderLayerHandle.Target is ILayer layer)
                        {
                            var renderContext = context?.RenderContext;
                            using (renderContext.PushTagAndRestore(InputExtensions.WindowInputSource, lastInputSource))
                            {
                                skiaRenderer.Layer = layer;
                                ((IGraphicsRendererBase)skiaRenderer).Draw(context);
                            }
                        }
                    }
                    else
                    {
                        Texture? tex = null;

                        if (cmd.TextureId != IntPtr.Zero)
                        {
                            textureHandle = GCHandle.FromIntPtr(cmd.TextureId);
                            if (textureHandle.Target is Texture texture)
                            {
                                tex = texture;
                            }
                        }
                        else
                        {
                            tex = fontTexture;
                        }
                        if (tex != null)
                        {
                            var is32Bits = false;
                            commandList.SetPipelineState(imPipeline);
                            commandList.SetVertexBuffer(0, vertexBinding.Buffer, 0, Unsafe.SizeOf<ImDrawVert>());
                            commandList.SetIndexBuffer(indexBinding.Buffer, 0, is32Bits);

                            commandList.SetScissorRectangle(
                                new Rectangle(
                                    (int)cmd.ClipRect.X,
                                    (int)cmd.ClipRect.Y,
                                    (int)(cmd.ClipRect.Z - cmd.ClipRect.X),
                                    (int)(cmd.ClipRect.W - cmd.ClipRect.Y)
                                )
                            );

                            imShader.Parameters.Set(ImGuiShader_DrawFXKeys.tex, tex);
                            imShader.Parameters.Set(ImGuiShader_DrawFXKeys.proj, ref projMatrix);
                            imShader.EffectInstance.Apply(graphicsContext);

                            commandList.DrawIndexed((int)cmd.ElemCount, idxOffset, vtxOffset);
                        }

                        idxOffset += (int)cmd.ElemCount;
                    }
                }
                vtxOffset += cmdList.VtxBuffer.Size;
            }
        }
    }
}