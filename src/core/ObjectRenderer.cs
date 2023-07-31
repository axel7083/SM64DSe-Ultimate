/*
    Copyright 2012 Kuribo64

    This file is part of SM64DSe.

    SM64DSe is free software: you can redistribute it and/or modify it under
    the terms of the GNU General Public License as published by the Free
    Software Foundation, either version 3 of the License, or (at your option)
    any later version.

    SM64DSe is distributed in the hope that it will be useful, but WITHOUT ANY 
    WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
    FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License along 
    with SM64DSe. If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Globalization;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using SM64DSe.ImportExport.Loaders.InternalLoaders;

namespace SM64DSe
{
    public class ObjectRenderer
    {
        [Pure]
        public static Color ColorFromString(string theString)
        {
            return Color.FromArgb((int)(uint.Parse(theString, NumberStyles.HexNumber) | 0xff000000));
        }
        
        public static ObjectRenderer FromLevelObject(LevelObject obj)
        {
            ObjectRenderer ret = null;
            
            ObjectDatabase.RendererConfig rendererConfig = ObjectDatabase.m_ObjectInfo[obj.ID].m_Renderer;
            if (rendererConfig == null)
            {
                throw new Exception("Received null render config.");
            }

            switch (rendererConfig.type)
            {
                case "NormalBMD":
                    ret = new NormalBMDRenderer(rendererConfig.GetFirstFile(), rendererConfig.scale);
                    break;
                case "NormalKCL":
                    ret = new NormalKCLRenderer(rendererConfig.GetFirstFile(), rendererConfig.scale);
                    break;
                case "DoubleBMD":
                    if (rendererConfig.offsets == null)
                        ret = new DoubleRenderer(
                            rendererConfig.GetFirstFile(), 
                            rendererConfig.GetSecondFile(), 
                            rendererConfig.scale
                            );
                    else
                        ret = new DoubleRenderer(
                            rendererConfig.GetFirstFile(), 
                            rendererConfig.GetSecondFile(),
                            rendererConfig.offsets[0], 
                            rendererConfig.offsets[1], 
                            rendererConfig.scale
                            );
                    break;
                case "Kurumajiku":
                    ret = new KurumajikuRenderer(
                        rendererConfig.GetFirstFile(), 
                        rendererConfig.GetSecondFile(), 
                        rendererConfig.scale
                        );
                    break;
                case "Pole":
                    ret = new PoleRenderer(
                        ColorFromString(rendererConfig.border), 
                        ColorFromString(rendererConfig.fill), obj.Parameters[0]
                        );
                    break;
                case "ColorCube":
                    ret = new ColorCubeRenderer(
                        ColorFromString(rendererConfig.border), 
                        ColorFromString(rendererConfig.fill), 
                        obj.SupportsRotation()
                        );
                    break;
                case "Player":
                    ret = new PlayerRenderer(rendererConfig.scale, rendererConfig.animation);
                    break;
                case "Luigi":
                    ret = new LuigiRenderer(rendererConfig.scale);
                    break;
                case "ChainedChomp":
                    ret = new ChainedWanWanRenderer();
                    break;
                case "Goomboss":
                    ret = new GoombossRender();
                    break;
                case "Tree":
                    ret = new TreeRenderer((obj.Parameters[0] >> 4) & 0x7);
                    break;
                case "Painting":
                    ret = new PaintingRenderer(obj.Parameters[0], obj.Parameters[1]);
                    break;
                case "UnchainedChomp":
                    ret = new LooseWanWanRenderer();
                    break;
                case "Fish":
                    ret = new FishRenderer(obj.Parameters[0] & 0xf);
                    break;
                case "Butterfly":
                    ret = new ButterflyRenderer();
                    break;
                case "Star":
                    ret = new StarRenderer(obj);
                    break;
                case "BowserSkyPlatform":
                    ret = new Koopa3bgRenderer(obj.Parameters[0] & 0xff);
                    break;
                case "BigSnowman":
                    ret = new BigSnowmanRenderer();
                    break;
                case "Toxbox":
                    ret = new ToxboxRenderer();
                    break;
                case "Pokey":
                    ret = new PokeyRenderer();
                    break;
                case "FlPuzzle":
                    ret = new FlPuzzleRenderer(obj.Parameters[0] & 0xff);
                    break;
                case "FlameThrower":
                    ret = new FlameThrowerRenderer(obj.Parameters[1]);
                    break;
                case "C1Trap":
                    ret = new C1TrapRenderer();
                    break;
                case "Wiggler":
                    ret = new WigglerRenderer();
                    break;
                case "Koopa":
                    ret = new KoopaRenderer((obj.Parameters[0] & 1) != 0);
                    break;
                case "KoopaShell":
                    ret = new KoopaShellRenderer((obj.Parameters[0] & 1) != 0);
                    break;
                default:
                    ret = new ColorCubeRenderer(Color.FromArgb(255, 0, 0), Color.FromArgb(64, 0, 0), obj.SupportsRotation());
                    break;
            }

            ret.m_ObjUniqueID = obj.m_UniqueID;

            return ret;
        }

        public virtual void Release() { }

        public virtual bool GottaRender(RenderMode mode) { return false; }
        public virtual void Render(RenderMode mode) { }
        public virtual void UpdateRenderer() { }

        public virtual string GetFilename() { return m_Filename; }
        public virtual Vector3 GetScale() { return m_Scale; }

        public uint m_ObjUniqueID;

        public string m_Filename;
        public Vector3 m_Scale;
    }

    class KoopaRenderer : NormalBMDRenderer
    {
        public KoopaRenderer(bool isRed)
        {
            Construct("data/enemy/nokonoko/nokonoko" + (isRed ? "_red" : "") + ".bmd", 0.008f);
        }
    }
    
    class KoopaShellRenderer : NormalBMDRenderer
    {
        public KoopaShellRenderer(bool isRed)
        {
            Construct("data/enemy/nokonoko/shell_" + (isRed ? "red" : "green") + ".bmd", 0.008f);
        }
    }

    class ColorCubeRenderer : ObjectRenderer
    {
        
        public ColorCubeRenderer(Color border, Color fill, bool showaxes)
        {
            m_BorderColor = border;
            m_FillColor = fill;
            m_ShowAxes = showaxes;
        }

        public override bool GottaRender(RenderMode mode)
        {
            if (mode == RenderMode.Translucent) return false;
            else return true;
        }

        public override void Render(RenderMode mode)
        {
            const float s = 0.04f;

            if (mode != RenderMode.Picking)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Color4(m_FillColor);
                GL.Disable(EnableCap.Lighting);
            }

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s, -s, -s);
            GL.Vertex3(-s, s, -s);
            GL.Vertex3(s, -s, -s);
            GL.Vertex3(s, s, -s);
            GL.Vertex3(s, -s, s);
            GL.Vertex3(s, s, s);
            GL.Vertex3(-s, -s, s);
            GL.Vertex3(-s, s, s);
            GL.Vertex3(-s, -s, -s);
            GL.Vertex3(-s, s, -s);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s, s, -s);
            GL.Vertex3(-s, s, s);
            GL.Vertex3(s, s, -s);
            GL.Vertex3(s, s, s);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s, -s, -s);
            GL.Vertex3(s, -s, -s);
            GL.Vertex3(-s, -s, s);
            GL.Vertex3(s, -s, s);
            GL.End();

            if (mode != RenderMode.Picking)
            {
                GL.LineWidth(2.0f);
                GL.Color4(m_BorderColor);

                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex3(s, s, s);
                GL.Vertex3(-s, s, s);
                GL.Vertex3(-s, s, -s);
                GL.Vertex3(s, s, -s);
                GL.Vertex3(s, s, s);
                GL.Vertex3(s, -s, s);
                GL.Vertex3(-s, -s, s);
                GL.Vertex3(-s, -s, -s);
                GL.Vertex3(s, -s, -s);
                GL.Vertex3(s, -s, s);
                GL.End();

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(-s, s, s);
                GL.Vertex3(-s, -s, s);
                GL.Vertex3(-s, s, -s);
                GL.Vertex3(-s, -s, -s);
                GL.Vertex3(s, s, -s);
                GL.Vertex3(s, -s, -s);
                GL.End();

                if (m_ShowAxes)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(1.0f, 0.0f, 0.0f);
                    GL.Vertex3(0.0f, 0.0f, 0.0f);
                    GL.Color3(1.0f, 0.0f, 0.0f);
                    GL.Vertex3(s * 2.0f, 0.0f, 0.0f);
                    GL.Color3(0.0f, 1.0f, 0.0f);
                    GL.Vertex3(0.0f, 0.0f, 0.0f);
                    GL.Color3(0.0f, 1.0f, 0.0f);
                    GL.Vertex3(0.0f, s * 2.0f, 0.0f);
                    GL.Color3(0.0f, 0.0f, 1.0f);
                    GL.Vertex3(0.0f, 0.0f, 0.0f);
                    GL.Color3(0.0f, 0.0f, 1.0f);
                    GL.Vertex3(0.0f, 0.0f, s * 2.0f);
                    GL.End();
                }
            }
        }


        private Color m_BorderColor, m_FillColor;
        private bool m_ShowAxes;
    }


    class PoleRenderer : ObjectRenderer
    {
        public PoleRenderer(Color border, Color fill, ushort param)
        {
            m_BorderColor = border;
            m_FillColor = fill;
            m_height = (byte)param * 0.01f;

        }

        public override bool GottaRender(RenderMode mode)
        {
            if (mode == RenderMode.Opaque) return false;
            else return true;
        }

        public override void Render(RenderMode mode)
        {
            const float s = 0.04f;

            if (mode != RenderMode.Picking)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Color4(Color.FromArgb(100, m_FillColor));
                GL.Disable(EnableCap.Lighting);
            }

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s, -s, -s);
            GL.Vertex3(-s, m_height, -s);
            GL.Vertex3(s, -s, -s);
            GL.Vertex3(s, m_height, -s);
            GL.Vertex3(s, -s, s);
            GL.Vertex3(s, m_height, s);
            GL.Vertex3(-s, -s, s);
            GL.Vertex3(-s, m_height, s);
            GL.Vertex3(-s, -s, -s);
            GL.Vertex3(-s, m_height, -s);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s, m_height, -s);
            GL.Vertex3(-s, m_height, s);
            GL.Vertex3(s, m_height, -s);
            GL.Vertex3(s, m_height, s);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s, -s, -s);
            GL.Vertex3(s, -s, -s);
            GL.Vertex3(-s, -s, s);
            GL.Vertex3(s, -s, s);
            GL.End();

            if (mode != RenderMode.Picking)
            {
                GL.LineWidth(2.0f);
                GL.Color4(m_BorderColor);

                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex3(s, m_height, s);
                GL.Vertex3(-s, m_height, s);
                GL.Vertex3(-s, m_height, -s);
                GL.Vertex3(s, m_height, -s);
                GL.Vertex3(s, m_height, s);
                GL.Vertex3(s, -s, s);
                GL.Vertex3(-s, -s, s);
                GL.Vertex3(-s, -s, -s);
                GL.Vertex3(s, -s, -s);
                GL.Vertex3(s, -s, s);
                GL.End();

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(-s, m_height, s);
                GL.Vertex3(-s, -s, s);
                GL.Vertex3(-s, m_height, -s);
                GL.Vertex3(-s, -s, -s);
                GL.Vertex3(s, m_height, -s);
                GL.Vertex3(s, -s, -s);
                GL.End();
            }
        }


        private Color m_BorderColor, m_FillColor;
        private float m_height;
    }

    class StarRenderer : ObjectRenderer
    {
        private bool m_showsStar = false;
        private NormalBMDRenderer m_ModelRenderer = null;
        private NormalBMDRenderer m_StarRenderer = null;

        public StarRenderer(LevelObject obj)
        {
            char startype = obj.Parameters[0].ToString("X4")[2];
            if (obj.ID == 63)
            {
                switch (startype)
                {
                    case '0':
                        m_ModelRenderer = new NormalBMDRenderer("data/normal_obj/star/star_base.bmd", 0.008f);
                        m_Filename = "data/normal_obj/star/star_base.bmd";
                        break;
                    case '1':
                        m_ModelRenderer = new NormalBMDRenderer("data/normal_obj/star_box/star_box.bmd", 0.008f);
                        m_Filename = "data/normal_obj/star_box/star_box.bmd";
                        break;
                    case '4':
                        m_ModelRenderer = new NormalBMDRenderer("data/normal_obj/star_box/star_box.bmd", 0.008f);
                        m_Filename = "data/normal_obj/star_box/star_box.bmd";
                        break;
                    case '6':
                        m_ModelRenderer = new NormalBMDRenderer("data/normal_obj/star_box/star_box.bmd", 0.008f);
                        m_Filename = "data/normal_obj/star_box/star_box.bmd";
                        m_showsStar = true;
                        break;
                    default:
                        m_showsStar = true;
                        break;
                }
            }
            else if (obj.ID == 61)
            {
                switch (startype)
                {
                    case '6':
                        m_ModelRenderer = new NormalBMDRenderer("data/normal_obj/star_box/star_box.bmd", 0.008f);
                        m_Filename = "data/normal_obj/star_box/star_box.bmd";
                        m_showsStar = true;
                        break;
                    default:
                        m_StarRenderer = new NormalBMDRenderer("data/normal_obj/star/obj_star.bmd", 0.008f);
                        m_Filename = "data/normal_obj/star/obj_star.bmd";
                        m_showsStar = true;
                        break;
                }
            }
            else
            {
                m_showsStar = true;
            }
        }

        public override bool GottaRender(RenderMode mode)
        {
            bool star = false;
            bool additional = false;
            if (m_showsStar)
                star = (m_StarRenderer != null) ? m_StarRenderer.GottaRender(mode) : mode != RenderMode.Opaque;
            
            additional = (m_ModelRenderer != null) ? m_ModelRenderer.GottaRender(mode) : false;
            return star || additional;
        }

        public override void Render(RenderMode mode)
        {
            if (m_showsStar)
            {
                if (m_StarRenderer != null)
                {
                    m_StarRenderer.Render(mode);
                }
                else
                {
                    const float s = 0.08f;

                    if (mode != RenderMode.Picking)
                    {
                        GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                        GL.BindTexture(TextureTarget.Texture2D, 0);
                        GL.Color4(Color.FromArgb(100, 255, 200, 0));
                        GL.Disable(EnableCap.Lighting);
                    }
                    GL.Begin(PrimitiveType.TriangleFan);
                    GL.Vertex3(0, 0, 0.25 * s);
                    GL.Vertex3(0, s, 0);
                    for (int i = 0; i <= 5; i++)
                    {
                        GL.Vertex3(Math.Sin(i * 1.25664) * s, Math.Cos(i * 1.25664) * s, 0);
                        GL.Vertex3(Math.Sin(i * 1.25664 + 0.62832) * s * 0.5, Math.Cos(i * 1.25664 + 0.62832) * s * 0.5, 0);
                    }
                    GL.End();

                    GL.Begin(PrimitiveType.TriangleFan);
                    GL.Vertex3(0, 0, -0.25 * s);
                    GL.Vertex3(0, s, 0);
                    for (int i = 4; i >= 0; i--)
                    {
                        GL.Vertex3(Math.Sin(i * 1.25664 + 0.62832) * s * 0.5, Math.Cos(i * 1.25664 + 0.62832) * s * 0.5, 0);
                        GL.Vertex3(Math.Sin(i * 1.25664) * s, Math.Cos(i * 1.25664) * s, 0);
                    }
                    GL.End();


                    if (mode != RenderMode.Picking)
                    {
                        GL.LineWidth(2.0f);
                        GL.Color4(Color.FromArgb(255, 200, 0));

                        GL.Begin(PrimitiveType.LineLoop);
                        GL.Vertex3(0, s, 0);
                        for (int i = 0; i < 5; i++)
                        {
                            GL.Vertex3(Math.Sin(i * 1.25664) * s, Math.Cos(i * 1.25664) * s, 0);
                            GL.Vertex3(Math.Sin(i * 1.25664 + 0.62832) * s * 0.5, Math.Cos(i * 1.25664 + 0.62832) * s * 0.5, 0);
                        }
                        GL.End();

                    }
                }
            }
            if (m_ModelRenderer != null) m_ModelRenderer.Render(mode);
        }

        public override void Release()
        {
            if (m_ModelRenderer != null) m_ModelRenderer.Release();
            if (m_StarRenderer != null) m_StarRenderer.Release();
        }
    }

    class ExitRenderer : ObjectRenderer
    {
        private float m_XScale, m_YScale, m_XRotation;

        public ExitRenderer(ushort param, ushort param2)
        {
            m_XScale = (float)(((param2 >> 8) & 0xF) + 1) * 0.1f;
            m_YScale = (float)(((param2 >> 12) & 0xF) + 1) * 0.1f;
            m_XRotation = 360.0f / 65536.0f * (float)param;

        }

        public override bool GottaRender(RenderMode mode)
        {
            if (mode == RenderMode.Opaque) return false;
            else return true;
        }

        public override void Render(RenderMode mode)
        {
            const float s = 0.02f;
            float halfWidth = m_XScale*0.5f;
            GL.Rotate(m_XRotation, 1f, 0f, 0f);

            if (mode != RenderMode.Picking)
            {
                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Color4(Color.FromArgb(100, 255, 0, 0));
                GL.Disable(EnableCap.Lighting);
            }

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-halfWidth, 0, -s);
            GL.Vertex3(-halfWidth, m_YScale, -s);
            GL.Vertex3(halfWidth, 0, -s);
            GL.Vertex3(halfWidth, m_YScale, -s);
            GL.Vertex3(halfWidth, 0, s);
            GL.Vertex3(halfWidth, m_YScale, s);
            GL.Vertex3(-halfWidth, 0, s);
            GL.Vertex3(-halfWidth, m_YScale, s);
            GL.Vertex3(-halfWidth, 0, -s);
            GL.Vertex3(-halfWidth, m_YScale, -s);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-halfWidth, m_YScale, -s);
            GL.Vertex3(-halfWidth, m_YScale, s);
            GL.Vertex3(halfWidth, m_YScale, -s);
            GL.Vertex3(halfWidth, m_YScale, s);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-halfWidth, 0, -s);
            GL.Vertex3(halfWidth, 0, -s);
            GL.Vertex3(-halfWidth, 0, s);
            GL.Vertex3(halfWidth, 0, s);
            GL.End();

            if (mode != RenderMode.Picking)
            {
                GL.LineWidth(2.0f);
                GL.Color4(Color.FromArgb(255,0,0));

                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex3(halfWidth, m_YScale, s);
                GL.Vertex3(-halfWidth, m_YScale, s);
                GL.Vertex3(-halfWidth, m_YScale, -s);
                GL.Vertex3(halfWidth, m_YScale, -s);
                GL.Vertex3(halfWidth, m_YScale, s);
                GL.Vertex3(halfWidth, 0, s);
                GL.Vertex3(-halfWidth, 0, s);
                GL.Vertex3(-halfWidth, 0, -s);
                GL.Vertex3(halfWidth, 0, -s);
                GL.Vertex3(halfWidth, 0, s);
                GL.End();

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(-halfWidth, m_YScale, s);
                GL.Vertex3(-halfWidth, 0, s);
                GL.Vertex3(-halfWidth, s, -s);
                GL.Vertex3(-halfWidth, 0, -s);
                GL.Vertex3(halfWidth, m_YScale, -s);
                GL.Vertex3(halfWidth, 0, -s);
                GL.End();
            }
        }
    }

    class DoorRenderer : ObjectRenderer
    {
        private DoorObject m_DoorObj;
        private NormalBMDRenderer m_MainRenderer, m_AuxRenderer;

        public DoorRenderer(DoorObject obj)
        {
            m_DoorObj = obj;
            int doortype = m_DoorObj.DoorType;

            m_MainRenderer = m_AuxRenderer = null;
            if ((doortype >= 1 && doortype <= 8) || doortype == 13 || doortype == 14 || (doortype >= 19 && doortype <= 23))
            {
                m_MainRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door0.bmd", 1f);
                switch (doortype)
                {
                    case 2: m_AuxRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door0_star.bmd", 1f); break;
                    case 3: 
                    case 13: m_AuxRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door0_star1.bmd", 1f); break;
                    case 4: 
                    case 14: m_AuxRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door0_star3.bmd", 1f); break;
                    case 5: m_AuxRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door0_star10.bmd", 1f); break;
                    case 6:
                    case 7:
                    case 19:
                    case 20:
                    case 21:
                    case 22: 
                    case 23: m_AuxRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door0_keyhole.bmd", 1f); break;
                }
            }
            else if (doortype >= 9 && doortype <= 12)
            {
                m_MainRenderer = new NormalBMDRenderer("data/normal_obj/stargate/obj_stargate.bmd", 1f);
            }
            else
                switch (doortype)
                {
                    case 15: m_MainRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door2_boro.bmd", 1f); break;
                    case 16: m_MainRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door3_tetsu.bmd", 1f); break;
                    case 17: m_MainRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door4_yami.bmd", 1f); break;
                    case 18: m_MainRenderer = new NormalBMDRenderer("data/normal_obj/door/obj_door5_horror.bmd", 1f); break;
                }
        }

        public override bool GottaRender(RenderMode mode)
        {
            if (m_DoorObj.DoorType == 0) return mode != RenderMode.Translucent; // cheat to give it priority
            else
            {
                bool main = (m_MainRenderer != null) ? m_MainRenderer.GottaRender(mode) : false;
                bool aux = (m_AuxRenderer != null) ? m_AuxRenderer.GottaRender(mode) : false;
                return main || aux;
            }
        }

        public override void Render(RenderMode mode)
        {
            if (m_DoorObj.DoorType == 0)
            {
                float sx = (m_DoorObj.PlaneSizeX + 1) * 0.05f;
                float sy = (m_DoorObj.PlaneSizeY + 1) * 0.05f;

                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);

                if (mode != RenderMode.Picking)
                {
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.Disable(EnableCap.Lighting);
                    GL.Color4(0f, 1f, 0f, 1f);

                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(sx, 0f, 0f);
                    GL.Vertex3(sx, sy * 2f, 0f);
                    GL.Vertex3(-sx, sy * 2f, 0f);
                    GL.Vertex3(-sx, 0f, 0f);
                    GL.End();

                    GL.Color4(0.75f, 1f, 0.75f, 0.75f);
                }

                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(sx, 0f, 0f);
                GL.Vertex3(sx, sy * 2f, 0f);
                GL.Vertex3(-sx, sy * 2f, 0f);
                GL.Vertex3(-sx, 0f, 0f);
                GL.End();

                if (mode != RenderMode.Picking)
                    GL.Color4(1f, 0.75f, 0.75f, 0.75f);

                GL.Begin(PrimitiveType.Quads);
                GL.Vertex3(-sx, 0f, 0f);
                GL.Vertex3(-sx, sy * 2f, 0f);
                GL.Vertex3(sx, sy * 2f, 0f);
                GL.Vertex3(sx, 0f, 0f);
                GL.End();
            }
            else
            {
                GL.Scale(0.008f, 0.008f, 0.008f);
                m_MainRenderer.Render(mode);
                if (m_DoorObj.DoorType >= 9 && m_DoorObj.DoorType <= 12)
                {
                    GL.Rotate(180f, 0f, 1f, 0f);
                    m_MainRenderer.Render(mode);
                }
                if (m_AuxRenderer != null) m_AuxRenderer.Render(mode);
            }
        }

        public override void Release()
        {
            if (m_MainRenderer != null) m_MainRenderer.Release();
            if (m_AuxRenderer != null) m_AuxRenderer.Release();
        }
    }


    class NormalBMDRenderer : ObjectRenderer
    {
        public NormalBMDRenderer() { }
        public NormalBMDRenderer(string filename, float scale)
        {
            Construct(filename, scale);
        }

        public override void Release()
        {
            ModelCache.RemoveModel(m_Model);
        }

        public override bool GottaRender(RenderMode mode)
        {
            int dl = 0;
            switch (mode)
            {
                case RenderMode.Opaque: dl = m_DisplayLists[0]; break;
                case RenderMode.Translucent: dl = m_DisplayLists[1]; break;
                case RenderMode.Picking: dl = m_DisplayLists[2]; break;
            }

            return dl != 0;
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(m_Scale);
            switch (mode)
            {
                case RenderMode.Opaque: GL.CallList(m_DisplayLists[0]); break;
                case RenderMode.Translucent: GL.CallList(m_DisplayLists[1]); break;
                case RenderMode.Picking: GL.CallList(m_DisplayLists[2]); break;
            }
        }

        public void Construct(string filename, float scale)
        {
            m_Model = ModelCache.GetModel(filename);
            m_DisplayLists = ModelCache.GetDisplayLists(m_Model);
            m_Scale = new Vector3(scale, scale, scale);
            m_Filename = filename;
        }

        public override void UpdateRenderer()
        {
            ModelCache.RemoveModel(m_Model);
            Construct(m_Filename, m_Scale.X);
        }

        private BMD m_Model;
        private int[] m_DisplayLists;
    }
    
    class NormalKCLRenderer : ObjectRenderer
    {
        public NormalKCLRenderer() { }
        public NormalKCLRenderer(string filename, float scale)
        {
            Construct(filename, scale);
            GetDisplayLists();
        }

        public override void Release()
        {
            GL.DeleteLists(m_KCLMeshDLists[0], 1); m_KCLMeshDLists[0] = 0;
            GL.DeleteLists(m_KCLMeshDLists[1], 1); m_KCLMeshDLists[1] = 0;
        }

        public override bool GottaRender(RenderMode mode)
        {
            int dl = 0;
            switch (mode)
            {
                case RenderMode.Opaque: dl = m_KCLMeshDLists[0]; break;
                case RenderMode.Translucent: dl = m_KCLMeshDLists[1]; break;
                case RenderMode.Picking: dl = m_KCLMeshDLists[0]; break;
            }

            return dl != 0;
        }

        public void GetDisplayLists()
        {
            m_KCLMeshPickingDLists[0] = GL.GenLists(1);
            GL.NewList(m_KCLMeshPickingDLists[0], ListMode.Compile);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            for (int i = 0; i < m_Planes.Count; i++)
            {
                GL.Begin(PrimitiveType.Triangles);
                GL.Color4(Color.FromArgb(i));
                GL.Vertex3(m_Planes[i].point1);
                GL.Vertex3(m_Planes[i].point2);
                GL.Vertex3(m_Planes[i].point3);
                GL.End();
            }
            GL.EndList();

            m_KCLMeshDLists[0] = GL.GenLists(1);
            GL.NewList(m_KCLMeshDLists[0], ListMode.Compile);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Enable(EnableCap.PolygonOffsetFill);
            GL.PolygonOffset(1f, 1f);
            for (int i = 0; i < m_Planes.Count; i++)
            {
                Color planeColour = m_Colours[m_Planes[i].type];

                GL.Begin(PrimitiveType.Triangles);
                GL.Color3(planeColour);
                GL.Vertex3(m_Planes[i].point1);
                GL.Vertex3(m_Planes[i].point2);
                GL.Vertex3(m_Planes[i].point3);
                GL.End();
            }
            GL.Disable(EnableCap.PolygonOffsetFill);
            GL.EndList();

            m_KCLMeshDLists[1] = GL.GenLists(1);
            GL.NewList(m_KCLMeshDLists[1], ListMode.Compile);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            for (int i = 0; i < m_Planes.Count; i++)
            {
                GL.Begin(PrimitiveType.LineLoop);
                GL.Color3(Color.Orange);
                GL.Vertex3(m_Planes[i].point1);
                GL.Vertex3(m_Planes[i].point2);
                GL.Vertex3(m_Planes[i].point3);
                GL.End();
            }
            GL.EndList();
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(m_Scale);
            switch (mode)
            {
                case RenderMode.Opaque: GL.CallList(m_KCLMeshDLists[0]); break;
                case RenderMode.Translucent: GL.CallList(m_KCLMeshDLists[1]); break;
                case RenderMode.Picking: GL.CallList(m_KCLMeshDLists[0]); break;
            }
        }

        public void Construct(string filename, float scale)
        {
            m_KCL = new KCL(Program.m_ROM.GetFileFromName(filename));
            m_Planes = m_KCL.m_Planes;
            m_Scale = new Vector3(scale, scale, scale);
            m_Filename = filename;
        }

        public override void UpdateRenderer()
        {
            Construct(m_Filename, m_Scale.X);
            GetDisplayLists();
        }

        private KCL m_KCL;
        private List<KCL.ColFace> m_Planes;
        private int[] m_KCLMeshPickingDLists = new int[1];
        private int[] m_KCLMeshDLists = new int[2];
        private KCLLoader.CollisionMapColours m_Colours = new KCLLoader.CollisionMapColours();

        /*
        
        */
    }


    class PlayerRenderer : ObjectRenderer
    {
        public PlayerRenderer() { }
        public PlayerRenderer(float scale, string animation)
        {
			m_Animation = new BCA(Program.m_ROM.GetFileFromName("data/player/" + animation));
			Construct(scale);
        }

        public override void Release()
        {
            ModelCache.RemoveModel(m_Model);
            ModelCache.RemoveModel(m_Head);
        }

        public override bool GottaRender(RenderMode mode)
        {
            int dl = 0;
            switch (mode)
            {
                case RenderMode.Opaque: dl = m_DisplayLists[0]; dl = m_DisplayListsHead[0]; break;
                case RenderMode.Translucent: dl = m_DisplayLists[1]; dl = m_DisplayListsHead[1]; break;
                case RenderMode.Picking: dl = m_DisplayLists[2]; dl = m_DisplayListsHead[2]; break;
            }

            return dl != 0;
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(m_Scale);
            switch (mode)
            {
                case RenderMode.Opaque:
					GL.CallList(m_DisplayLists[0]);
					GL.PushMatrix();
					var mtx = Matrix4.CreateTranslation(0, 1, 0);
					GL.MultMatrix(ref m_HeadTransform);
					GL.CallList(m_DisplayListsHead[0]);
					GL.PopMatrix();
					break;
                case RenderMode.Translucent:
					GL.CallList(m_DisplayLists[1]);
					GL.PushMatrix();
					GL.MultMatrix(ref m_HeadTransform);
					GL.CallList(m_DisplayListsHead[1]);
					GL.PopMatrix();
					break;
                case RenderMode.Picking:
					GL.CallList(m_DisplayLists[2]);
					GL.PushMatrix();
					GL.MultMatrix(ref m_HeadTransform);
					GL.CallList(m_DisplayListsHead[2]);
					GL.PopMatrix();
					break;
            }
        }

        public virtual void Construct(float scale)
        {
            m_Model = ModelCache.GetModel("data/player/mario_model.bmd");
            m_Head = ModelCache.GetModel("data/player/mario_head_cap.bmd");
			

			m_DisplayLists = ModelCache.GetDisplayLists(m_Model, m_Animation,0);
            m_DisplayListsHead = ModelCache.GetDisplayLists(m_Head);


			m_HeadTransform = m_Animation.GetAllMatricesForFrame(m_Model.m_ModelChunks, 0)[15];
			m_Scale = new Vector3(scale, scale, scale);
            m_Filename = "data/player/mario_model.bmd";
        }

        public override void UpdateRenderer()
        {
            ModelCache.RemoveModel(m_Model);
            ModelCache.RemoveModel(m_Head);
            Construct(m_Scale.X);
        }

        protected BMD m_Model;
		protected BMD m_Head;
		protected BCA m_Animation;
		protected Matrix4 m_HeadTransform;
		protected int[] m_DisplayLists;
		protected int[] m_DisplayListsHead;
    }

	class LuigiRenderer : PlayerRenderer
	{
		public LuigiRenderer(){ }
		public LuigiRenderer(float scale):
			base(scale, "wait.bca")
		{
		}

		public override void Construct(float scale)
		{
			m_Model = ModelCache.GetModel("data/player/luigi_model.bmd");
			m_Head = ModelCache.GetModel("data/player/luigi_head_cap.bmd");


			m_DisplayLists = ModelCache.GetDisplayLists(m_Model, m_Animation, 0);
			m_DisplayListsHead = ModelCache.GetDisplayLists(m_Head);


			m_HeadTransform = m_Animation.GetAllMatricesForFrame(m_Model.m_ModelChunks, 0)[15];
			m_Scale = new Vector3(scale, scale, scale);
			m_Filename = "data/player/luigi_model.bmd";
		}
	}


	class PaintingRenderer : ObjectRenderer
    {
        private float m_XScale, m_YScale, m_XRotation;

        private bool m_Mirrored;

        private int m_GLTextureID;

        public PaintingRenderer(ushort param, ushort param2)
        {
            string[] ptgnames = { "for_bh", "for_bk", "for_ki", "for_sm", "for_cv_ex5", "for_fl", "for_dl", "for_wl",
                                  "for_sl", "for_wc", "for_hm", "for_hs", "for_td_tt", "for_ct", "for_ex_mario", "for_ex_luigi",
                                  "for_ex_wario", "for_vs_cross", "for_vs_island" };
            int ptgid = (param >> 8) & 0x1F;
            if (ptgid > 18) ptgid = 18;
            string filename = "data/picture/" + ptgnames[ptgid] + ".bmd";
            m_XScale = (float)((param & 0xF) + 1) * 0.1f;
            m_YScale = (float)(((param >> 4) & 0xF) + 1)* 0.1f;
            m_Mirrored = (((param >> 13) & 0x3) == 3);

            SM64DSFormats.NitroTexture texture = new BMD(Program.m_ROM.GetFileFromName(filename)).m_Textures.First().Value;

            m_XRotation = 360.0f / 65536.0f*(float)param2;

            m_GLTextureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, m_GLTextureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, (int)texture.m_Width, (int)texture.m_Height,
                0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, texture.GetARGB());

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
        }

        public override bool GottaRender(RenderMode mode)
        {
            if (mode == RenderMode.Translucent) return false;
            else return true;
        }

        public override void Render(RenderMode mode)
        {
            GL.Rotate(m_XRotation, 1f, 0f, 0f);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Disable(EnableCap.Lighting);
            if (mode != RenderMode.Picking)
            {
                GL.BindTexture(TextureTarget.Texture2D, m_GLTextureID);
                GL.Color4(Color.White);
            }

            float halfWidth = m_XScale * 0.5f;

            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(m_Mirrored ? 1 : 0, 1);
            GL.Vertex3(-halfWidth, 0, 0);

            GL.TexCoord2(m_Mirrored ? 0 : 1, 1);
            GL.Vertex3(halfWidth, 0, 0);

            GL.TexCoord2(m_Mirrored ? 0 : 1, 0);
            GL.Vertex3(halfWidth, m_YScale, 0);

            GL.TexCoord2(m_Mirrored ? 1 : 0, 0);
            GL.Vertex3(-halfWidth, m_YScale, 0);

            GL.End();
        }
    }

    class TreeRenderer : NormalBMDRenderer
    {
        public TreeRenderer(int treetype)
        {
            string[] treenames = { "bomb", "toge", "yuki", "yashi", "castle", "castle", "castle", "castle" };
            if (treetype > 7) treetype = 7;
            string filename = "data/normal_obj/tree/" + treenames[treetype] + "_tree.bmd";
            Construct(filename, 0.008f);
        }
    }

    class KurumajikuRenderer : ObjectRenderer
    {
        private NormalBMDRenderer m_KurumaRenderer, m_KurumajikuRenderer;

        public KurumajikuRenderer(string file1, string file2, float scale)
        {
            m_KurumaRenderer = new NormalBMDRenderer(file1, scale);
            m_KurumajikuRenderer = new NormalBMDRenderer(file2, scale);
            m_Filename = m_KurumaRenderer.m_Filename + ";" + m_KurumajikuRenderer.m_Filename;
        }

        public override void Release()
        {
            m_KurumaRenderer.Release();
            m_KurumajikuRenderer.Release();
        }

        public override bool GottaRender(RenderMode mode)
        {
            return m_KurumaRenderer.GottaRender(mode) || m_KurumajikuRenderer.GottaRender(mode);
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(0.008f, 0.008f, 0.008f);
            m_KurumajikuRenderer.Render(mode);

            GL.Translate(50f, 0f, 37.5f);
            m_KurumaRenderer.Render(mode);
            GL.Translate(-50f, 50f, 0f);
            m_KurumaRenderer.Render(mode);
            GL.Translate(-50f, -50f, 0f);
            m_KurumaRenderer.Render(mode);
            GL.Translate(50f, -50f, 0f);
            m_KurumaRenderer.Render(mode);
        }
    }

    class Koopa3bgRenderer : NormalBMDRenderer
    {
        public Koopa3bgRenderer(int npart)
        {
            string partnames = "abcdefghij";
            if (npart > 9) npart = 9;
            string filename = "data/special_obj/kb3_stage/kb3_" + partnames[npart] + ".bmd";
            Construct(filename, 0.008f);
        }
    }

    class ToxboxRenderer : NormalBMDRenderer
    {
        public ToxboxRenderer()
            : base("data/enemy/onimasu/onimasu.bmd", 0.008f)
        {
        }

        public override void Render(RenderMode mode)
        {
            GL.Translate(0.0f, 0.25f, 0.0f);
            base.Render(mode);
        }
    }

    class C1TrapRenderer : NormalBMDRenderer
    {
        public C1TrapRenderer()
            : base("data/special_obj/c1_trap/c1_trap.bmd", 1f)
        {
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(0.008f, 0.008f, 0.008f);
            base.Render(mode);
            GL.Translate(-44f, 0f, 0f);
            base.Render(mode);
        }
    }

    class FlPuzzleRenderer : NormalBMDRenderer
    {
        public FlPuzzleRenderer(int npart)
        {
            if (npart > 13)
                npart = 13;
            string filename = "data/special_obj/fl_puzzle/fl_14_" + npart.ToString("D2") + ".bmd";
            Construct(filename, 0.008f);
        }
    }

    class BigSnowmanRenderer : DoubleRenderer
    {
        public BigSnowmanRenderer() : base("data/enemy/big_snowman/big_snowman_body.bmd", "data/enemy/big_snowman/big_snowman_head.bmd", 1f) { }

        public override void Render(RenderMode mode)
        {
            GL.Scale(0.012f, 0.012f, 0.012f);
            GL.Translate(0f, 5f, 0f);
            m_PrimaryRenderer.Render(mode);

            GL.Translate(0f, 25f, 0f);
            m_SecondaryRenderer.Render(mode);
        }
    }

    class PokeyRenderer : ObjectRenderer
    {
        private NormalBMDRenderer m_HeadRenderer, m_BodyRenderer;

        public PokeyRenderer()
        {
            m_HeadRenderer = new NormalBMDRenderer("data/enemy/sanbo/sanbo_head.bmd", 1f);
            m_BodyRenderer = new NormalBMDRenderer("data/enemy/sanbo/sanbo_body.bmd", 1f);
            this.m_Filename = m_HeadRenderer.m_Filename + ";" + m_BodyRenderer.m_Filename;
        }

        public override void Release()
        {
            m_HeadRenderer.Release();
            m_BodyRenderer.Release();
        }

        public override bool GottaRender(RenderMode mode)
        {
            return m_HeadRenderer.GottaRender(mode) || m_BodyRenderer.GottaRender(mode);
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(0.008f, 0.008f, 0.008f);
            GL.Translate(0f, 5f, 0f);
            m_BodyRenderer.Render(mode);
            GL.Translate(0f, 15f, 0f);
            m_BodyRenderer.Render(mode);
            GL.Translate(0f, 15f, 0f);
            m_BodyRenderer.Render(mode);
            GL.Translate(0f, 15f, 0f);
            m_BodyRenderer.Render(mode);

            GL.Translate(0f, 15f, 0f);
            m_HeadRenderer.Render(mode);
        }
    }

    class WigglerRenderer : ObjectRenderer
    {
        private NormalBMDRenderer m_HeadRenderer;
        private NormalBMDRenderer[] m_BodyRenderer = new NormalBMDRenderer[4];

        public WigglerRenderer()
        {
            m_HeadRenderer = new NormalBMDRenderer("data/enemy/hanachan/hanachan_head.bmd", 1f);
            this.m_Filename = m_HeadRenderer.m_Filename + ";";
            for (int i = 0; i < 4; i++)
            {
                string name = "data/enemy/hanachan/hanachan_body0" + (i + 1) + ".bmd";
                m_BodyRenderer[i] = new NormalBMDRenderer(name, 1f);
                this.m_Filename += name + ";";
            }
        }

        public override void Release()
        {
            m_HeadRenderer.Release();
            foreach (NormalBMDRenderer renderer in m_BodyRenderer)
                renderer.Release();
        }

        public override bool GottaRender(RenderMode mode)
        {
            return m_HeadRenderer.GottaRender(mode);
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(0.008f, 0.008f, 0.008f);

            m_HeadRenderer.Render(mode);

            foreach (NormalBMDRenderer renderer in m_BodyRenderer)
            {
                GL.Translate(0f, 0f, -15f);
                renderer.Render(mode);
            }
        }
    }

    class LooseWanWanRenderer : ObjectRenderer
    {
        private NormalBMDRenderer m_BodyRenderer, m_ChainRenderer;

        public LooseWanWanRenderer()
        {
            m_BodyRenderer = new NormalBMDRenderer("data/enemy/wanwan/wanwan.bmd", 1f);
            m_ChainRenderer = new NormalBMDRenderer("data/enemy/wanwan/chain.bmd", 1f);
            this.m_Filename = m_BodyRenderer.m_Filename + ";" + m_ChainRenderer.m_Filename;
        }

        public override void Release()
        {
            m_BodyRenderer.Release();
            m_ChainRenderer.Release();
        }

        public override bool GottaRender(RenderMode mode)
        {
            return m_BodyRenderer.GottaRender(mode) || m_ChainRenderer.GottaRender(mode);
        }

        public override void Render(RenderMode mode)
        {
            GL.PushMatrix();
            GL.Scale(0.008f, 0.008f, 0.008f);
            for (int i = 0; i < 6; i++)
            {
                GL.Translate(0f, 3.25f, 10f);
                m_ChainRenderer.Render(mode);
            }

            GL.Translate(0f, 3.25f, 40f);
            m_BodyRenderer.Render(mode);
            GL.PopMatrix();
        }
    }

    class ChainedWanWanRenderer : LooseWanWanRenderer
    {
        private NormalBMDRenderer m_PoleRenderer;

        public ChainedWanWanRenderer()
        {
            m_PoleRenderer = new NormalBMDRenderer("data/normal_obj/obj_pile/pile.bmd", 0.008f);
            this.m_Filename = base.m_Filename + ";" + m_PoleRenderer.m_Filename;
        }

        public override void Release()
        {
            base.Release();
            m_PoleRenderer.Release();
        }

        public override bool GottaRender(RenderMode mode)
        {
            return base.GottaRender(mode) || m_PoleRenderer.GottaRender(mode);
        }

        public override void Render(RenderMode mode)
        {
            GL.PushMatrix();
            m_PoleRenderer.Render(mode);
            GL.PopMatrix();

            base.Render(mode);
        }
    }

    class GoombossRender : ObjectRenderer
    {
        NormalBMDRenderer m_bmdRenderer;
        public GoombossRender()
        {
            m_bmdRenderer = new NormalBMDRenderer("data/enemy/kuriking/kuriking_model.bmd", 0.01f);
        }

        public override bool GottaRender(RenderMode mode)
        {
            return m_bmdRenderer.GottaRender(mode);
        }

        public override void Render(RenderMode mode)
        {
            if (mode != RenderMode.Picking)
            {
                GL.Color4(Color.White);
                GL.LineWidth(4);
                GL.Begin(PrimitiveType.LineLoop);
                for (int i = 0; i < 16; i++)
                {
                    GL.Vertex3(Math.Sin(Math.PI / 8 * i) * 1.4, 0, Math.Cos(Math.PI / 8 * i) * 1.4);
                }
                GL.End();
            }
            GL.PushMatrix();
            GL.Rotate(360/16*13, Vector3.UnitY);
            GL.Translate(1.4, 0, 0);
            m_bmdRenderer.Render(mode);
            GL.PopMatrix();
        }

        public override void Release()
        {
            m_bmdRenderer.Release();
        }
    }

    class DoubleRenderer : ObjectRenderer
    {
        protected NormalBMDRenderer m_PrimaryRenderer, m_SecondaryRenderer;
        Vector3 m_OffsetFirst, m_OffsetSecond;
        float scale;

        public DoubleRenderer(String first, String second, Vector3 offsetFirst, Vector3 offsetSecond, float scale)
        {
            m_PrimaryRenderer = new NormalBMDRenderer(first, 1f);
            m_SecondaryRenderer = new NormalBMDRenderer(second, 1f);
            m_OffsetFirst = offsetFirst;
            m_OffsetSecond = offsetSecond;
            this.scale = scale;
            this.m_Filename = first + ";" + second;
        }

        public DoubleRenderer(String first, String second, float scale) :
            this(first, second, Vector3.Zero, Vector3.Zero, scale) { }

        public override void Release()
        {
            m_PrimaryRenderer.Release();
            m_SecondaryRenderer.Release();
        }

        public override bool GottaRender(RenderMode mode)
        {
            return m_SecondaryRenderer.GottaRender(mode) || m_PrimaryRenderer.GottaRender(mode);
        }

        public override void Render(RenderMode mode)
        {
            GL.Scale(scale, scale, scale);
            GL.Translate(m_OffsetFirst.X, m_OffsetFirst.Y, m_OffsetFirst.Z);
            m_PrimaryRenderer.Render(mode);
            GL.Translate(m_OffsetSecond.X, m_OffsetSecond.Y, m_OffsetSecond.Z);
            m_SecondaryRenderer.Render(mode);
        }
    }

    class ColourArrowRenderer : ObjectRenderer
    {
        float rotX;
        float rotY;
        float rotZ;

        public ColourArrowRenderer(Color border, Color fill, bool showaxes)
        {
            m_BorderColor = border;
            m_FillColor = fill;
            m_ShowAxes = showaxes;

            rotX = 0.0f;
            rotY = 0.0f;
            rotZ = 0.0f;
        }

        public ColourArrowRenderer(Color border, Color fill, bool showaxes, float rotX = 0, float rotY = 0, float rotZ = 0)
        {
            m_BorderColor = border;
            m_FillColor = fill;
            m_ShowAxes = showaxes;

            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
        }

        public override bool GottaRender(RenderMode mode)
        {
            if (mode == RenderMode.Translucent) return false;
            else return true;
        }

        public override void Render(RenderMode mode)
        {
            const float s = 0.04f;

            if (mode != RenderMode.Picking)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Color4(m_FillColor);
                GL.Disable(EnableCap.Lighting);
            }

            GL.Rotate(rotX, Vector3d.UnitX);
            GL.Rotate(rotY, Vector3d.UnitY);
            GL.Rotate(rotZ, Vector3d.UnitZ);

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(0, -s, -s * 1.5f);
            GL.Vertex3(0, s, -s * 1.5f);
            GL.Vertex3(s, -s, s);
            GL.Vertex3(s, s, s);
            GL.Vertex3(-s, -s, s);
            GL.Vertex3(-s, s, s);
            GL.Vertex3(0, -s, -s * 1.5f);
            GL.Vertex3(0, s, -s * 1.5f);
            GL.End();

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(0, s, -s * 1.5f);
            GL.Vertex3(-s, s, s);
            GL.Vertex3(s, s, s);
            GL.End();

            GL.Begin(PrimitiveType.Triangles);
            GL.Vertex3(s, -s, s);
            GL.Vertex3(-s, -s, s);
            GL.Vertex3(0, -s, -s * 1.5f);
            GL.End();

            if (mode != RenderMode.Picking)
            {
                GL.LineWidth(3f);
                GL.Color4(m_BorderColor);

                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex3(s, s, s);
                GL.Vertex3(-s, s, s);
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(0, s, -s * 1.5f);
                GL.Color4(m_BorderColor);
                GL.Vertex3(s, s, s);
                GL.Vertex3(s, -s, s);
                GL.Vertex3(-s, -s, s);
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(0, -s, -s * 1.5f);
                GL.Color4(m_BorderColor);
                GL.Vertex3(s, -s, s);
                GL.End();

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(-s, s, s);
                GL.Vertex3(-s, -s, s);
                GL.Color3(0.0f, 1.0f, 0.0f);
                GL.Vertex3(0, s, -s * 1.5f);
                GL.Vertex3(0, -s, -s * 1.5f);
                GL.Color4(m_BorderColor);
                GL.End();

                if (m_ShowAxes)
                {
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(1.0f, 0.0f, 0.0f);
                    GL.Vertex3(0.0f, 0.0f, 0.0f);
                    GL.Color3(1.0f, 0.0f, 0.0f);
                    GL.Vertex3(s * 2.0f, 0.0f, 0.0f);
                    GL.Color3(0.0f, 1.0f, 0.0f);
                    GL.Vertex3(0.0f, 0.0f, 0.0f);
                    GL.Color3(0.0f, 1.0f, 0.0f);
                    GL.Vertex3(0.0f, s * 2.0f, 0.0f);
                    GL.Color3(0.0f, 0.0f, 1.0f);
                    GL.Vertex3(0.0f, 0.0f, 0.0f);
                    GL.Color3(0.0f, 0.0f, 1.0f);
                    GL.Vertex3(0.0f, 0.0f, s * 2.0f);
                    GL.End();
                }
            }
        }

        private Color m_BorderColor, m_FillColor;
        private bool m_ShowAxes;
    }

    class ButterflyRenderer : NormalBMDRenderer
    {
        public ButterflyRenderer()
            : base("data/normal_obj/butterfly/butterfly.bmd", 1f)
        {
        }

        public override void Render(RenderMode mode)
        {
            GL.PushMatrix();
            GL.Scale(0.008f, 0.008f, 0.008f);

            GL.Translate(10, 10, 10);
            base.Render(mode);
            GL.Translate(-18, 5, -12);
            GL.Rotate(45, Vector3.UnitX);
            base.Render(mode);
            GL.Translate(15, 0, 8);
            GL.Rotate(-90, Vector3.UnitX);
            base.Render(mode);
            GL.PopMatrix();
        }
    }

    class FishRenderer : NormalBMDRenderer
    {
        int m_count;
        public FishRenderer(int count)
            : base("data/normal_obj/fish/fish.bmd", 1f)
        {
            m_count = count;
        }

        public override void Render(RenderMode mode)
        {
            GL.PushMatrix();
            GL.Scale(0.008f, 0.008f, 0.008f);

            for (int i = 0; i < m_count; i++)
            {
                GL.Rotate(360 / m_count, Vector3.UnitY);
                GL.Translate(10, 0, 0);
                base.Render(mode);
            }
            GL.PopMatrix();
        }
    }

    class FlameThrowerRenderer : ObjectRenderer
    {
        private float m_XRotation;
        public FlameThrowerRenderer(ushort param2)
        {
            m_XRotation = 360.0f / 65536.0f * (float)param2;
        }

        public override bool GottaRender(RenderMode mode)
        {
            if (mode == RenderMode.Opaque) return false;
            else return true;
        }

        public override void Render(RenderMode mode)
        {
            const float s1 = 0.04f;
            const float s2 = 0.12f;
            const float length = 1.4f;

            if (mode != RenderMode.Picking)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Color4(Color.FromArgb(200,127,0,0));
                GL.Disable(EnableCap.Lighting);
            }
            GL.PushMatrix();
            GL.Translate(0, 0.04, 0);
            GL.Rotate(m_XRotation, 1f, 0f, 0f);

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s1, -s1, 0);
            GL.Vertex3(-s1, s1, 0);
            GL.Vertex3(s1, -s1, 0);
            GL.Vertex3(s1, s1, 0);
            GL.Vertex3(s2, -s2, length);
            GL.Vertex3(s2, s2, length);
            GL.Vertex3(-s2, -s2, length);
            GL.Vertex3(-s2, s2, length);
            GL.Vertex3(-s1, -s1, 0);
            GL.Vertex3(-s1, s1, 0);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s1, s1, 0);
            GL.Vertex3(-s2, s2, length);
            GL.Vertex3(s1, s1, 0);
            GL.Vertex3(s2, s2, length);
            GL.End();

            GL.Begin(PrimitiveType.TriangleStrip);
            GL.Vertex3(-s1, -s1, 0);
            GL.Vertex3(s1, -s1, 0);
            GL.Vertex3(-s2, -s2, length);
            GL.Vertex3(s2, -s2, length);
            GL.End();
            
            if (mode != RenderMode.Picking)
            {
                GL.LineWidth(2.0f);
                GL.Color4(Color.FromArgb(255,0,0));

                GL.Begin(PrimitiveType.LineStrip);
                GL.Vertex3(s2, s2, length);
                GL.Vertex3(-s2, s2, length);
                GL.Vertex3(-s1, s1, 0);
                GL.Vertex3(s1, s1, 0);
                GL.Vertex3(s2, s2, length);
                GL.Vertex3(s2, -s2, length);
                GL.Vertex3(-s2, -s2, length);
                GL.Vertex3(-s1, -s1, 0);
                GL.Vertex3(s1, -s1, 0);
                GL.Vertex3(s2, -s2, length);
                GL.End();

                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(-s2, s2, length);
                GL.Vertex3(-s2, -s2, length);
                GL.Vertex3(-s1, s1, 0);
                GL.Vertex3(-s1, -s1, 0);
                GL.Vertex3(s1, s1, 0);
                GL.Vertex3(s1, -s1, 0);
                GL.End();
            }
            GL.PopMatrix();
        }
    }
}
