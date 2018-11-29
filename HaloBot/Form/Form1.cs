using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace HaloBot
{
    public partial class Form1 : Form
    {		
        public MemoryReaderWriter gameState;
        public Navigation nav;
		public Graph graph;
        private AIHandler ai;
        private String autofilename;
        public String launchPath;

        private Device m_device = null;
        private PresentParameters present_params;
        private bool mLostDevice = false;
        private Microsoft.DirectX.Direct3D.Font nodesFont;

        private int prevCommandIndex;
        private LinkedList<String> prevCommands;
        private Structures.FLOAT3 scrollTo;
        private Point oldHover;
        private bool dragging;
        private bool hotKeysPressed;
        private ushort lastNodePlaced;
        private bool autoLink;


        public Form1(String filename)
        {
            //init
            InitializeComponent();
            launchPath = Path.GetDirectoryName(Application.ExecutablePath);
            autofilename = filename;
            textBox5.KeyDown += new KeyEventHandler(textBox5_KeyDown);
            prevCommands = new LinkedList<String>();
            prevCommandIndex = 0;
            autoLink = false;

            //delegates
            WriteAIDelegate AIOut = new WriteAIDelegate(WriteAI);
            ToggleAIButtonsDelegate ToggleDel = new ToggleAIButtonsDelegate(ToggleAIButtons);

            //init major components
            gameState = new MemoryReaderWriter();
            gameState.Stopped += new MemoryReaderWriter.ProcessStoppedHandler(gameState_Stopped);
            gameState.Start();
            graph = new Graph(500);
            graph.Modified += new Graph.GraphModifiedHandler(graph_Modified);
            nav = new Navigation(graph, gameState);
            ai = new AIHandler(this, AIOut, ToggleDel);

            //graphics window
            drawPanel.Size = clickpanel.Size;
            drawPanel.Location = clickpanel.Location;
            InitGraphics();
            clickpanel.Paint += new PaintEventHandler(clickpanel_Paint);
            clickpanel.MouseDown += new MouseEventHandler(clickpanel_MouseDown);
            clickpanel.MouseMove += new MouseEventHandler(clickpanel_MouseMove);
            clickpanel.MouseWheel += new MouseEventHandler(clickpanel_MouseWheel);
            clickpanel.MouseUp += new MouseEventHandler(clickpanel_MouseUp);
            clickpanel.MouseLeave += new EventHandler(clickpanel_MouseLeave);
        }

        //load the form, initialize components
        private void Form1_Load(object sender, EventArgs e)
        {
            this.AcceptButton = button4;
            textBox6.Text = "type \"help\" for a list of commands\r\n";
            this.Text = "GuiltySpark - Untitled*";
            if (autofilename != null)
                OpenFile(autofilename);
            #region aboutLabel
            aboutLabel.Text =
                "GuiltySpark\n" +
                "Version 1.0.43\n" +
                "For Halo Custom Edition 1.08\n\n" +

                "By Connor Sauve (Conscars)";
            #endregion

            clickpanel.Invalidate();
        }

        void clickpanel_MouseLeave(object sender, EventArgs e)
        {
            dragging = false;
            this.Cursor = Cursors.Default;
            clickpanel.Invalidate();
        }

        void clickpanel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
            this.Cursor = Cursors.Default;
            clickpanel.Invalidate();
        }

        void clickpanel_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                zoomSelector.DownButton();
            else
                zoomSelector.UpButton();
            clickpanel.Invalidate();
        }

        void clickpanel_MouseMove(object sender, MouseEventArgs e)
        {
            this.clickpanel.Focus();
            double dX = e.Location.X - oldHover.X;
            double dY = e.Location.Y - oldHover.Y;

            if (e.Button == MouseButtons.Right & dragging)
            {
                autoRotateCheckBox.Checked = false;

                double newAngle = (double)horizontalAngleUpDown.Value + dX;
                if (newAngle > 360)
                    newAngle -= 360;
                else if (newAngle < 0)
                    newAngle += 360;
                horizontalAngleUpDown.Value = (decimal)newAngle;

                if ((double)verticalAngleUpDown.Value + dY <= 90 && (double)verticalAngleUpDown.Value + dY >= 0)
                    verticalAngleUpDown.Value += (int)dY;

                clickpanel.Invalidate();
            }
            if (e.Button == MouseButtons.Left & dragging)
            {
                checkBox1.Checked = false;

                if (GetKeyState(VirtualKeyStates.VK_SHIFT) < 0)
                {
                    dY *= 1 / (double)zoomSelector.Value;
                    scrollTo.Z += (float)dY;
                }
                else
                {
                    dX *= 1 / (double)zoomSelector.Value;
                    dY *= 1 / (double)zoomSelector.Value;

                    double angle = Math.Atan2(dX, dY)
                        - ((double)horizontalAngleUpDown.Value / 360) * (Math.PI * 2)
                        + Math.PI / 2;
                    double radius = Math.Sqrt(dX * dX + dY * dY);

                    dX = radius * Math.Cos(angle);
                    dY = radius * Math.Sin(angle);

                    scrollTo.X += (float)dX;
                    scrollTo.Y += (float)dY;
                }

                clickpanel.Invalidate();
            }
            oldHover = e.Location;
        }

        void clickpanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragging = true;
                this.Cursor = Cursors.Hand;
            }
            if (e.Button == MouseButtons.Right)
            {
                dragging = true;
                this.Cursor = Cursors.Cross;
            }
            clickpanel.Invalidate();
        }
        
        void dxDrawLine(Device m_device, int x0, int y0, int x1, int y1, Color c)
        {
            CustomVertex.TransformedColored[] points = new CustomVertex.TransformedColored[2];
            points[0].X = (float)x0;
            points[0].Y = (float)y0;
            points[0].Color = c.ToArgb();
            points[1].X = (float)x1;
            points[1].Y = (float)y1;
            points[1].Color = c.ToArgb();

            m_device.VertexFormat = CustomVertex.TransformedColored.Format;
            m_device.DrawUserPrimitives(PrimitiveType.LineList, 1, points);
        }

        void dxDrawCircle(Device m_device, int x, int y, int r, int sides, Color c)
        {
            CustomVertex.TransformedColored[] points = new CustomVertex.TransformedColored[sides + 1];
            double angleStep = 2 * Math.PI / sides;
            double angle = 0;
            for (int i = 0; i <= sides; i++)
            {
                points[i].Color = c.ToArgb();
                points[i].X = (float)(r * Math.Cos(angle) + x);
                points[i].Y = (float)(r * Math.Sin(angle) + y);
                angle += angleStep;
            }

            m_device.VertexFormat = CustomVertex.TransformedColored.Format;
            m_device.DrawUserPrimitives(PrimitiveType.LineStrip, sides, points);
        }

        void dxFillCircle(Device m_device, int x, int y, int r, int sides, Color c)
        {
            CustomVertex.TransformedColored[] points = new CustomVertex.TransformedColored[sides + 2];
            points[0].X = (float)x;
            points[0].Y = (float)y;
            points[0].Color = c.ToArgb();

            double angleStep = 2 * Math.PI / sides;
            double angle = 0;
            for (int i = 1; i <= sides + 1; i++)
            {
                points[i].Color = c.ToArgb();
                points[i].X = (float)(r * Math.Cos(angle) + x);
                points[i].Y = (float)(r * Math.Sin(angle) + y);
                angle += angleStep;
            }
            m_device.VertexFormat = CustomVertex.TransformedColored.Format;
            m_device.DrawUserPrimitives(PrimitiveType.TriangleFan, sides, points);
        }

        void dxDrawPolygon(Device m_device, Point[] inPoints, int sides, Color c)
        {
            CustomVertex.TransformedColored[] points = new CustomVertex.TransformedColored[sides + 1];

            for (int i = 0; i <= sides; i++)
            {
                points[i].Color = c.ToArgb();
                if (i == sides)
                {
                    points[i].X = inPoints[0].X;
                    points[i].Y = inPoints[0].Y;
                }
                else
                {
                    points[i].X = inPoints[i].X;
                    points[i].Y = inPoints[i].Y;
                }
                
            }
            
            m_device.VertexFormat = CustomVertex.TransformedColored.Format;
            m_device.DrawUserPrimitives(PrimitiveType.LineStrip, sides, points);
        }

        void dxDrawText(String s, int x, int y, Color c)
        {
            nodesFont.DrawText(null, s, x, y, c);
        }
        

        //------------------------------------------------------------------
        //handles the drawing of the node graph
        //------------------------------------------------------------------
        #region drawing
        void clickpanel_Paint(object sender, PaintEventArgs e)
        {
            if (!CheckDevice())
            {
                return;
            }
            try
            {
                m_device.BeginScene();
                clickpanel.SuspendLayout();

                m_device.Clear(ClearFlags.Target, 0, 1.0f, 0);
                float vecX;
                Structures.FLOAT3 pos;
                Color myColor;
                if (checkBox5.Checked)
                {
                    myColor = Color.Yellow;
                    vecX = gameState.CameraHorizontalViewAngle;
                    pos = gameState.CameraPosition;
                }
                else
                {
                    myColor = Color.White;
                    vecX = gameState.PlayerHorizontalViewAngle;
                    pos = gameState.LocalPosition;
                }

                //change the view angle if autoRotate is checked
                if (gameState.processRunning && autoRotateCheckBox.Checked)
                {
                    int degrees = 360 - (int)((vecX / (2 * Math.PI)) * 360) + 90;
                    if (degrees > 360)
                        degrees -= 360;
                    horizontalAngleUpDown.Value = degrees;
                }

                //if "follow player" checked
                if (checkBox1.Checked)
                {
                    if (!gameState.processRunning && graph.pool[1] != null)
                    {
                        scrollTo.X = graph.pool[1].pos.X;
                        scrollTo.Y = graph.pool[1].pos.Y;
                        scrollTo.Z = graph.pool[1].pos.Z;
                    }
                    else
                    {
                        scrollTo.X = pos.X;
                        scrollTo.Y = pos.Y;
                        scrollTo.Z = pos.Z;
                    }
                }

                //highlight the current path's nodes
                Waypoint[] drawingArray = nav.CurrentPath.ToArray();
                Color pathColor = Color.Orange;
                for (int i = 0; i < drawingArray.Length; i++)
                {
                    Point temp = TransformCoordinate(drawingArray[i].pos);
                    int radius = (int)graphScaleSelector.Value;
                    dxFillCircle(m_device, temp.X, temp.Y, 2 * radius - 1, 10, pathColor);
                    pathColor = Color.FromArgb(pathColor.R - 256 / (drawingArray.Length + 1), pathColor.G, pathColor.B + 256 / (drawingArray.Length + 1));
                }

                #region for_every_node
                for (int i = 1; i <= graph.LastIndex; i++)
                {
                    if (graph.pool[i] != null)
                    {
                        Waypoint node = graph.pool[i];

                        #region draw_links
                        if (checkBox4.Checked)
                        {
                            Color linkColor;
                            for (int link = 0; link < node.NumberOfConnections; link++)
                            {
                                Waypoint dstWpt = graph.pool[node.SurroundingIndexes[link]];
                                Point srcPnt = TransformCoordinate(node.pos);
                                Point dst = TransformCoordinate(dstWpt.pos);

                                switch (graph.pool[i].ConnectionTypes[link])
                                {
                                    case 2: //teleport
                                        linkColor = Color.Lime;
                                        break;
                                    case 3: //jump
                                        linkColor = Color.HotPink;
                                        break;
                                    case 4: //crouch
                                        linkColor = Color.Orange;
                                        break;
                                    case 5: //look-ahead
                                        linkColor = Color.Red;
                                        break;
                                    default: //standard
                                        linkColor = Color.DarkSlateBlue;
                                        break;
                                }

                                dxDrawPolygon(m_device, GetArrow(srcPnt, dst), 3, linkColor);
                            }
                        }
                        #endregion draw_links

                        //draw the node's circle
                        if (checkBox2.Checked)
                        {
                            int radius = (int)graphScaleSelector.Value;
                            Point pnt = TransformCoordinate(node.pos);
                            dxDrawCircle(m_device, pnt.X, pnt.Y, radius * 2 - 1, 10, Color.MediumVioletRed);
                        }
                    }

                }


                //draw the numbers last so they dont get covered up by links and circles
                if (checkBox3.Checked)
                {
                    for (int i = 1; i <= graph.LastIndex; i++)
                    {
                        if (graph.pool[i] != null)
                        {
                            Point textPoint = TransformCoordinate(graph.pool[i].pos);
                            textPoint.Offset(-10, -5);

                            dxDrawText(i.ToString(), textPoint.X, textPoint.Y, Color.White);
                        }
                    }
                }


                #endregion for_every_node

                #region draw_you

                if (gameState.processRunning)
                {
                    Point localPlayer = TransformCoordinate(pos);
                    double viewAngleH = ((double)horizontalAngleUpDown.Value / 360) * (Math.PI * 2);

                    Point p1 = new Point((int)(localPlayer.X + 10 * Math.Cos(-vecX - viewAngleH)),
                                         (int)(localPlayer.Y + 10 * Math.Sin(-vecX - viewAngleH)));
                    Point p2 = new Point((int)(localPlayer.X + 20 * Math.Cos(-vecX - viewAngleH)),
                                         (int)(localPlayer.Y + 20 * Math.Sin(-vecX - viewAngleH)));

                    dxDrawLine(m_device, p1.X, p1.Y, p2.X, p2.Y, myColor);
                    dxDrawCircle(m_device, localPlayer.X, localPlayer.Y, 10, 10, myColor);
                    dxDrawText(checkBox5.Checked ? "cam" : "you", localPlayer.X - 8, localPlayer.Y - 6, myColor);
                }

                #endregion

                #region debug
                if (gameState.hitEdges != null)
                {
                    foreach (uint edge in gameState.hitEdges)
                    {
                        uint startIndex = gameState.edges[edge].startVertexIndex;
                        uint endIndex = gameState.edges[edge].endVertexIndex;
                        Point a = TransformCoordinate(gameState.vertices[startIndex].position);
                        Point b = TransformCoordinate(gameState.vertices[endIndex].position);
                        dxDrawLine(m_device, a.X, a.Y, b.X, b.Y, Color.Red);
                    }
                }
                Point ab = TransformCoordinate(pos);
                Point bb = TransformCoordinate(gameState.PlayerPosition(nav.aimbot.GetTargetIndex()));
                dxDrawLine(m_device, ab.X, ab.Y, bb.X, bb.Y, Color.Blue);

                Point pb = TransformCoordinate(gameState.hitPoint);
                dxFillCircle(m_device, pb.X, pb.Y, 4, 3, Color.Orange);

                #endregion

                m_device.EndScene();
                m_device.Present();
                clickpanel.ResumeLayout();
            }
            catch (DeviceLostException)
            {
                mLostDevice = true;
            }
        }

        private bool CheckDevice()
        {
            if (m_device.Disposed)
                return false;

            if (mLostDevice)
            {
                RecoverDevice();
                return false;
            }
            return true;
        }

        private void RecoverDevice()
        {
            int result;
            m_device.CheckCooperativeLevel(out result);

            Debug.WriteLine("m_device cooperative level: " + (Microsoft.DirectX.Direct3D.ResultCode)result);
            switch ((Microsoft.DirectX.Direct3D.ResultCode)result)
            {
                case Microsoft.DirectX.Direct3D.ResultCode.DeviceLost:
                    Debug.WriteLine("Device Lost");
                    mLostDevice = true;
                    break;
                case Microsoft.DirectX.Direct3D.ResultCode.Success:
                    mLostDevice = false;
                    break;
                case Microsoft.DirectX.Direct3D.ResultCode.DeviceNotReset:
                    try
                    {
                        m_device.Reset(present_params);
                    }
                    catch (DeviceLostException)
                    {
                        mLostDevice = true;
                    }
                    break;
            }
        }

        //gets the array of points needed to draw an arrow between two circles
        private Point[] GetArrow(Point src, Point dst)
        {
            int linkDrawSize = (int)graphScaleSelector.Value;
            int forwardOffset = linkDrawSize * 2;

            double angle = Math.Atan2((dst.Y - src.Y), (dst.X - src.X));
            Point base1 = new Point((int)(src.X + linkDrawSize * Math.Cos(angle - Math.PI / 2)),
                                    (int)(src.Y + linkDrawSize * Math.Sin(angle - Math.PI / 2)));
            Point base2 = new Point((int)(src.X + linkDrawSize * Math.Cos(angle + Math.PI / 2)),
                                    (int)(src.Y + linkDrawSize * Math.Sin(angle + Math.PI / 2)));

            Point offsetBase = new Point((int)(forwardOffset * Math.Cos(angle)),
                                         (int)(forwardOffset * Math.Sin(angle)));
            base1.Offset(offsetBase);
            base2.Offset(offsetBase);
            dst.Offset(-offsetBase.X, -offsetBase.Y);
            Point[] triangle = { base1, base2, dst };
            return triangle;
        }

        private Point[] GetArrow2(Point src, Point dst)
        {
            int lineWidthOffset = 2;
            int lineForwardOffset = 10;

            double angle = Math.Atan2((dst.Y - src.Y), (dst.X - src.X));
            Point start = new Point((int)(src.X + lineWidthOffset * Math.Cos(angle + Math.PI / 2)),
                                    (int)(src.Y + lineWidthOffset * Math.Sin(angle + Math.PI / 2)));

            Point end = new Point((int)(dst.X + lineWidthOffset * Math.Cos(angle + Math.PI / 2)),
                                  (int)(dst.Y + lineWidthOffset * Math.Sin(angle + Math.PI / 2)));

            Point offsetBase = new Point((int)(lineForwardOffset * Math.Cos(angle)),
                                         (int)(lineForwardOffset * Math.Sin(angle)));
            start.Offset(offsetBase);
            end.Offset(-offsetBase.X, -offsetBase.Y);
            Point[] triangle = { start, end };
            return triangle;
        }

        //transforms coordinates from game space to 2d graph view space
        private Point TransformCoordinate(Structures.FLOAT3 pos)
        {
            float x1 = pos.X;
            float y1 = pos.Y;
            float z1 = pos.Z;

            x1 -= scrollTo.X;
            y1 -= scrollTo.Y;
            z1 -= scrollTo.Z;
            x1 *= (float)zoomSelector.Value;
            y1 *= (float)zoomSelector.Value;

            //calculate the view angles
            double viewAngleV = ((double)verticalAngleUpDown.Value / 360) * (Math.PI * 2);
            double viewAngleH = ((double)horizontalAngleUpDown.Value / 360) * (Math.PI * 2);

            //rotate the vector by viewAngleH
            double cartesianX = x1 * Math.Cos(viewAngleH) - y1 * Math.Sin(viewAngleH);
            double cartesianY = x1 * Math.Sin(viewAngleH) + y1 * Math.Cos(viewAngleH);

            //apply the vertical scaling and offset
            cartesianY *= (float)Math.Sin(viewAngleV);
            cartesianY += (float)Math.Cos(viewAngleV) * z1 * (float)zoomSelector.Value;

            return new Point((int)(cartesianX + clickpanel.Width / 2), (int)(-cartesianY + clickpanel.Height / 2));
        }


        private void InitGraphics()
        {
            present_params = new PresentParameters();
            present_params.Windowed = true;
            present_params.SwapEffect = SwapEffect.Flip;

            m_device = new Device(0, DeviceType.Hardware, drawPanel, CreateFlags.MixedVertexProcessing, present_params);
            VertexBuffer vb = new VertexBuffer(m_device, 1024, Usage.None, VertexFormats.Normal, Pool.Default);
            m_device.SetStreamSource(5, vb, 0);

            nodesFont = new Microsoft.DirectX.Direct3D.Font(m_device,
                new System.Drawing.Font("Arial", 8f, FontStyle.Regular));
        }

        #endregion drawing

        private void gameState_Stopped(object sender, EventArgs e)
        {
            InterruptThreads();
        }

        //stop automated activity and add '*' to filename
        private void graph_Modified(object sender, EventArgs e)
        {
            InterruptThreads();
            if (!this.Text.EndsWith("*"))
                this.Text += "*";
            clickpanel.Invalidate();
        }

        [DllImport("user32.dll")]
        public static extern short GetKeyState(VirtualKeyStates nVirtKey);

        public enum VirtualKeyStates : int
        {
            VK_LBUTTON = 0x01,
            VK_RBUTTON = 0x02,
            VK_CANCEL = 0x03,
            VK_MBUTTON = 0x04,
            //
            VK_XBUTTON1 = 0x05,
            VK_XBUTTON2 = 0x06,
            //
            VK_BACK = 0x08,
            VK_TAB = 0x09,
            //
            VK_CLEAR = 0x0C,
            VK_RETURN = 0x0D,
            //
            VK_SHIFT = 0x10,
            VK_CONTROL = 0x11,
            VK_MENU = 0x12,
            VK_PAUSE = 0x13,
            VK_CAPITAL = 0x14,
            //
            VK_KANA = 0x15,
            VK_HANGEUL = 0x15,  /* old name - should be here for compatibility */
            VK_HANGUL = 0x15,
            VK_JUNJA = 0x17,
            VK_FINAL = 0x18,
            VK_HANJA = 0x19,
            VK_KANJI = 0x19,
            //
            VK_ESCAPE = 0x1B,
            //
            VK_CONVERT = 0x1C,
            VK_NONCONVERT = 0x1D,
            VK_ACCEPT = 0x1E,
            VK_MODECHANGE = 0x1F,
            //
            VK_SPACE = 0x20,
            VK_PRIOR = 0x21,
            VK_NEXT = 0x22,
            VK_END = 0x23,
            VK_HOME = 0x24,
            VK_LEFT = 0x25,
            VK_UP = 0x26,
            VK_RIGHT = 0x27,
            VK_DOWN = 0x28,
            VK_SELECT = 0x29,
            VK_PRINT = 0x2A,
            VK_EXECUTE = 0x2B,
            VK_SNAPSHOT = 0x2C,
            VK_INSERT = 0x2D,
            VK_DELETE = 0x2E,
            VK_HELP = 0x2F,
            //
            VK_LWIN = 0x5B,
            VK_RWIN = 0x5C,
            VK_APPS = 0x5D,
            //
            VK_SLEEP = 0x5F,
            //
            VK_NUMPAD0 = 0x60,
            VK_NUMPAD1 = 0x61,
            VK_NUMPAD2 = 0x62,
            VK_NUMPAD3 = 0x63,
            VK_NUMPAD4 = 0x64,
            VK_NUMPAD5 = 0x65,
            VK_NUMPAD6 = 0x66,
            VK_NUMPAD7 = 0x67,
            VK_NUMPAD8 = 0x68,
            VK_NUMPAD9 = 0x69,
            VK_MULTIPLY = 0x6A,
            VK_ADD = 0x6B,
            VK_SEPARATOR = 0x6C,
            VK_SUBTRACT = 0x6D,
            VK_DECIMAL = 0x6E,
            VK_DIVIDE = 0x6F,
            VK_F1 = 0x70,
            VK_F2 = 0x71,
            VK_F3 = 0x72,
            VK_F4 = 0x73,
            VK_F5 = 0x74,
            VK_F6 = 0x75,
            VK_F7 = 0x76,
            VK_F8 = 0x77,
            VK_F9 = 0x78,
            VK_F10 = 0x79,
            VK_F11 = 0x7A,
            VK_F12 = 0x7B,
            VK_F13 = 0x7C,
            VK_F14 = 0x7D,
            VK_F15 = 0x7E,
            VK_F16 = 0x7F,
            VK_F17 = 0x80,
            VK_F18 = 0x81,
            VK_F19 = 0x82,
            VK_F20 = 0x83,
            VK_F21 = 0x84,
            VK_F22 = 0x85,
            VK_F23 = 0x86,
            VK_F24 = 0x87,
            //
            VK_NUMLOCK = 0x90,
            VK_SCROLL = 0x91,
            //
            VK_OEM_NEC_EQUAL = 0x92,   // '=' key on numpad
            //
            VK_OEM_FJ_JISHO = 0x92,   // 'Dictionary' key
            VK_OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
            VK_OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
            VK_OEM_FJ_LOYA = 0x95,   // 'Left OYAYUBI' key
            VK_OEM_FJ_ROYA = 0x96,   // 'Right OYAYUBI' key
            //
            VK_LSHIFT = 0xA0,
            VK_RSHIFT = 0xA1,
            VK_LCONTROL = 0xA2,
            VK_RCONTROL = 0xA3,
            VK_LMENU = 0xA4,
            VK_RMENU = 0xA5,
            //
            VK_BROWSER_BACK = 0xA6,
            VK_BROWSER_FORWARD = 0xA7,
            VK_BROWSER_REFRESH = 0xA8,
            VK_BROWSER_STOP = 0xA9,
            VK_BROWSER_SEARCH = 0xAA,
            VK_BROWSER_FAVORITES = 0xAB,
            VK_BROWSER_HOME = 0xAC,
            //
            VK_VOLUME_MUTE = 0xAD,
            VK_VOLUME_DOWN = 0xAE,
            VK_VOLUME_UP = 0xAF,
            VK_MEDIA_NEXT_TRACK = 0xB0,
            VK_MEDIA_PREV_TRACK = 0xB1,
            VK_MEDIA_STOP = 0xB2,
            VK_MEDIA_PLAY_PAUSE = 0xB3,
            VK_LAUNCH_MAIL = 0xB4,
            VK_LAUNCH_MEDIA_SELECT = 0xB5,
            VK_LAUNCH_APP1 = 0xB6,
            VK_LAUNCH_APP2 = 0xB7,
            //
            VK_OEM_1 = 0xBA,   // ';:' for US
            VK_OEM_PLUS = 0xBB,   // '+' any country
            VK_OEM_COMMA = 0xBC,   // ',' any country
            VK_OEM_MINUS = 0xBD,   // '-' any country
            VK_OEM_PERIOD = 0xBE,   // '.' any country
            VK_OEM_2 = 0xBF,   // '/?' for US
            VK_OEM_3 = 0xC0,   // '`~' for US
            //
            VK_OEM_4 = 0xDB,  //  '[{' for US
            VK_OEM_5 = 0xDC,  //  '\|' for US
            VK_OEM_6 = 0xDD,  //  ']}' for US
            VK_OEM_7 = 0xDE,  //  ''"' for US
            VK_OEM_8 = 0xDF,
            //
            VK_OEM_AX = 0xE1,  //  'AX' key on Japanese AX kbd
            VK_OEM_102 = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
            VK_ICO_HELP = 0xE3,  //  Help key on ICO
            VK_ICO_00 = 0xE4,  //  00 key on ICO
            //
            VK_PROCESSKEY = 0xE5,
            //
            VK_ICO_CLEAR = 0xE6,
            //
            VK_PACKET = 0xE7,
            //
            VK_OEM_RESET = 0xE9,
            VK_OEM_JUMP = 0xEA,
            VK_OEM_PA1 = 0xEB,
            VK_OEM_PA2 = 0xEC,
            VK_OEM_PA3 = 0xED,
            VK_OEM_WSCTRL = 0xEE,
            VK_OEM_CUSEL = 0xEF,
            VK_OEM_ATTN = 0xF0,
            VK_OEM_FINISH = 0xF1,
            VK_OEM_COPY = 0xF2,
            VK_OEM_AUTO = 0xF3,
            VK_OEM_ENLW = 0xF4,
            VK_OEM_BACKTAB = 0xF5,
            //
            VK_ATTN = 0xF6,
            VK_CRSEL = 0xF7,
            VK_EXSEL = 0xF8,
            VK_EREOF = 0xF9,
            VK_PLAY = 0xFA,
            VK_ZOOM = 0xFB,
            VK_NONAME = 0xFC,
            VK_PA1 = 0xFD,
            VK_OEM_CLEAR = 0xFE
        }


        //------------------------------------------------------------------
		//update the UI and perform repeated tasks
        //------------------------------------------------------------------
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabPage1)
            {
                if (gameState.processRunning)
                {
                    toolStripStatusLabel1.Text = "Halo is running";

                    #region hotkeys

                    if (GetKeyState(VirtualKeyStates.VK_DELETE) < 0 && nav.GetWindow().Equals("Halo"))
                    {
                        if (!hotKeysPressed)
                        {
                            hotKeysPressed = true;
                            WriteOutput(graph.Remove(nav.GetClosestWaypoint(gameState.LocalPosition)) ?
                                "Successfully deleted nearest node" : "Failed to delete nearest node");

                            clickpanel.Invalidate();
                        }
                        
                    }
                    else if (GetKeyState(VirtualKeyStates.VK_INSERT) < 0 && nav.GetWindow().Equals("Halo"))
                    {
                        if (!hotKeysPressed)
                        {
                            Structures.FLOAT3 pos = checkBox5.Checked ? gameState.CameraPosition : gameState.LocalPosition;

                            hotKeysPressed = true;
                            if (autoLink)
                            {
                                lastNodePlaced = graph.Add(pos, lastNodePlaced);
                                WriteOutput(lastNodePlaced != 0 ?
                                        "Successfully added and linked node " + lastNodePlaced : "Failed to add node");
                            }
                            else
                            {
                                ushort n;
                                WriteOutput((n = graph.Add(pos)) != 0 ?
                                        "Successfully added node " + n : "Failed to add node");
                            }
                            clickpanel.Invalidate();
                        }
                    }
                    else if (GetKeyState(VirtualKeyStates.VK_HOME) < 0 && nav.GetWindow().Equals("Halo"))
                    {
                        if (!hotKeysPressed && !autoLink)
                        {
                            hotKeysPressed = true;
                            autoLink = true;
                            lastNodePlaced = 0;
                            WriteOutput("Auto-link enabled");
                        }
                    }
                    else if (GetKeyState(VirtualKeyStates.VK_END) < 0 && nav.GetWindow().Equals("Halo"))
                    {
                        if (!hotKeysPressed && autoLink)
                        {
                            hotKeysPressed = true;
                            autoLink = false;
                            WriteOutput("Auto-link disabled");
                        }
                    }
                    else
                    {
                        hotKeysPressed = false;
                    }
                    #endregion hotkeys

                }
                else
                {
                    toolStripStatusLabel1.Text = "Halo is not running";
                }

                //keep graph view up to date when ingame (nothing to invalidate it otherwise)
                if (nav.GetWindow().Equals("Halo"))
                    clickpanel.Invalidate();                

            }
            else
            {
                //in the AI tab
            }

            //GLOBAL HOTKEYS
            if (GetKeyState(VirtualKeyStates.VK_F11) < 0 && nav.GetWindow().Equals("Halo"))
            {
                if (!hotKeysPressed)
                {
                    hotKeysPressed = true;
                    WriteAI("Starting AI");
                    ai.Start();
                    clickpanel.Invalidate();
                }
            }
            else if (GetKeyState(VirtualKeyStates.VK_F12) < 0 && nav.GetWindow().Equals("Halo"))
            {
                if (!hotKeysPressed)
                {
                    hotKeysPressed = true;
                    WriteAI("Stopping AI");
                    ai.Stop();
                    clickpanel.Invalidate();
                }
            }
            else
            {
                hotKeysPressed = false;
            }

        }


        //------------------------------------------------------------------
		//handle a text command
        //------------------------------------------------------------------
		private void button4_Click(object sender, EventArgs e)
		{
            String command = textBox5.Text;
			if (command != "")
			{
                prevCommandIndex = 0;
                string[] cmd;

                if (command.StartsWith("!"))
                {
                    if (command.Equals("!!"))
                        command = prevCommands.Last.Value;
                    else
                    {
                        int result;
                        if (!int.TryParse(command.Substring(1), out result))
                        {
                            WriteOutput("Invalid command index: " + command.Substring(1));
                            return;
                        }
                        result = prevCommands.Count - result;
                        if (result < 0 || result > prevCommands.Count - 1)
                        {
                            WriteOutput("Command index out of range: " + command.Substring(1));
                            return;
                        }
                        command = prevCommands.ElementAt(result);
                    }
                }
                else if (!command.Equals("history"))
                {
                    if (prevCommands.Count > 9)
                        prevCommands.RemoveFirst();
                    prevCommands.AddLast(command);
                }

                WriteOutput(">> " + command);
                cmd = command.Split(" ".ToCharArray());

                if (cmd[0].Equals("add"))
                {
                    ushort added;
                    if (gameState.processRunning &&
                        (added = graph.Add(checkBox5.Checked ? gameState.CameraPosition : gameState.LocalPosition))
                        != 0)
                        WriteOutput("Successfully added node " + added);
                    else
                        WriteOutput("Failed to add node");
                }
                else if (cmd[0].Equals("delete") && cmd.Length == 2)
                {
                    ushort result;
                    if (ushort.TryParse(cmd[1], out result) && graph.Remove(result))
                        WriteOutput("Successfully deleted node " + result);
                    else
                        WriteOutput("Invalid argument(s)");
                }
                else if (cmd[0].Equals("move") && cmd.Length == 2)
                {
                    ushort result;
                    if (ushort.TryParse(cmd[1], out result) && graph.Move(result, gameState.LocalPosition))
                        WriteOutput("Successfully moved node " + result);
                    else
                        WriteOutput("Invalid argument(s)");
                }
                else if (cmd[0].Equals("isolate") && cmd.Length > 1)
                {
                    ushort result;
                    for (int i = 1; i < cmd.Length; i++)
                    {
                        if (ushort.TryParse(cmd[i], out result))
                        {
                            graph.RemoveLinks(result);
                            graph.pool[result].UnlinkAll();
                        }
                        else
                            WriteOutput("Invalid argument(s)");
                    }
                }
                else if (cmd[0].Equals("unlink") && cmd.Length == 3)
                {
                    ushort result;
                    ushort result2;
                    if (ushort.TryParse(cmd[1], out result) && ushort.TryParse(cmd[2], out result2))
                    {
                        graph.pool[result].Unlink(result2);
                        graph.pool[result2].Unlink(result);
                        WriteOutput("Successfully unlinked nodes " + result + " and " + result2);
                    }
                    else
                        WriteOutput("Invalid argument(s)");
                }
                else if (cmd[0].Equals("link") && cmd.Length == 4)
                {
                    ushort result;
                    ushort result2;
                    byte result3;
                    if (ushort.TryParse(cmd[1], out result) && ushort.TryParse(cmd[2], out result2)
                        && byte.TryParse(cmd[3], out result3))
                    {
                        WriteOutput(graph.Link(result, result2, result3)
                            ? "Linking successful" : "Linking failed");
                    }
                    else
                        WriteOutput("Invalid argument(s)");
                }
                else if (cmd[0].Equals("qlink") && cmd.Length > 2)
                {
                    ushort result;
                    ushort result2;
                    for (int i = 1; i < cmd.Length; i++)
                    {
                        if (ushort.TryParse(cmd[i], out result))
                        {
                            for (int j = 1; j < cmd.Length; j++)
                            {
                                if (ushort.TryParse(cmd[j], out result2))
                                    if (graph.Link(result, result2, 1))
                                        WriteOutput("Linking successfull");
                            }
                        }
                        else
                            WriteOutput("Invalid argument(s)");
                    }
                }
                else if (cmd[0].Equals("clink") && cmd.Length > 2)
                {
                    ushort result;
                    ushort result2;
                    for (int i = 1; i < cmd.Length - 1; i++)
                    {
                        if (ushort.TryParse(cmd[i], out result) && ushort.TryParse(cmd[i + 1], out result2))
                        {
                            graph.Link(result, result2, 1);
                            graph.Link(result2, result, 1);
                        }
                        else
                            WriteOutput("Invalid argument(s)");
                    }
                }
                else if (cmd[0].Equals("where") && cmd.Length == 2)
                {
                    ushort result;
                    if (ushort.TryParse(cmd[1], out result))
                    {
                        if (graph.pool[result] != null)
                        {
                            WriteOutput("Moving graph view to node " + result);
                            scrollTo.X = (int)graph.pool[result].pos.X;
                            scrollTo.Y = (int)graph.pool[result].pos.Y;
                            scrollTo.Z = (int)graph.pool[result].pos.Z;
                            checkBox1.Checked = false;
                        }
                        else
                            WriteOutput("Node " + result + " does not exist");
                    }
                    else
                        WriteOutput("Invalid argument(s)");
                }
                else if (cmd[0].Equals("goto") && cmd.Length == 2)
                {
                    InterruptThreads();
                    while (nav.Walking) { }

                    ushort dstIndex;
                    if (ushort.TryParse(cmd[1], out dstIndex) && graph.CheckParameterValid(dstIndex))
                    {
                        nav.WalkTo(dstIndex, false);
                    }
                    else
                        WriteOutput("Invalid argument(s)");
                }
                else if (cmd[0].Equals("altgoto") && cmd.Length == 2)
                {
                    InterruptThreads();
                    while (nav.Walking) { }

                    ushort dstIndex;
                    if (ushort.TryParse(cmd[1], out dstIndex) && graph.CheckParameterValid(dstIndex))
                    {
                        nav.WalkTo(dstIndex, true);
                    }
                    else
                        WriteOutput("Invalid argument(s)");
                }
                else if (cmd[0].Equals("help"))
                {
                    WriteOutput("add");
                    WriteOutput("  adds a node at your current position");
                    WriteOutput("delete <#>");
                    WriteOutput("  deletes the specified node");
                    WriteOutput("move <#>");
                    WriteOutput("  moves specified node to current position");
                    WriteOutput("link <src #> <dst #> <type #>");
                    WriteOutput("  creates one-way link from src to dst of type:");
                    WriteOutput("  1 = standard (purple)");
                    WriteOutput("  2 = teleport (green)");
                    WriteOutput("  3 = jump (pink)");
                    WriteOutput("  4 = crouch (orange)");
                    WriteOutput("  5 = look-ahead (red)");
                    WriteOutput("clink <#> <#> ...");
                    WriteOutput("  co-links all given nodes in order");
                    WriteOutput("qlink <#> <#> ...");
                    WriteOutput("  co-links all given nodes to each other");
                    WriteOutput("isolate <#> ...");
                    WriteOutput("  removes links to and from the given nodes");
                    WriteOutput("unlink <#> <#>");
                    WriteOutput("  removes links between the given nodes");
                    WriteOutput("where <#>");
                    WriteOutput("  moves the graph view to the given node");
                    WriteOutput("goto <#>");
                    WriteOutput("  manually walks the player to a node");
                    WriteOutput("altgoto <#>");
                    WriteOutput("  walks to the goal using an alternate path");
                    WriteOutput("stop");
                    WriteOutput("  manually stops controlling the player");
                    WriteOutput("deleteall");
                    WriteOutput("  deletes all nodes");
                    WriteOutput("deleteiso");
                    WriteOutput("  deletes all isolated nodes");
                    WriteOutput("cls");
                    WriteOutput("  clears this output window");
                    WriteOutput("history");
                    WriteOutput("  lists previously executed commands");
                    WriteOutput("!!");
                    WriteOutput("  re-execute the last command");
                    WriteOutput("!<#>");
                    WriteOutput("  re-execute the <#>th last command");
                    WriteOutput("help");
                    WriteOutput("  displays this message");
                }
                else if (cmd[0].Equals("stop"))
                {
                    InterruptThreads();
                }
                else if (cmd[0].Equals("deleteall"))
                {
                    WriteOutput("Deleting all nodes");
                    InterruptThreads();
                    graph = new Graph(500);
                    nav.SetNewGraphReference(graph);
                    graph.Modified += new Graph.GraphModifiedHandler(graph_Modified);
                    this.Text = "GuiltySpark - Untitled*";
                }
                else if (cmd[0].Equals("deleteiso"))
                {
                    for (ushort i = 1; i <= graph.LastIndex; i++)
                    {
                        if (graph.pool[i] != null && graph.pool[i].NumberOfConnections == 0)
                        {
                            WriteOutput("Deleting isolated node " + i);
                            graph.Remove(i);
                        }
                    }
                }
                else if (cmd[0].Equals("history"))
                {
                    for (int i = prevCommands.Count; i > 0; i--)
                        WriteOutput(i + ": " + prevCommands.ElementAt(prevCommands.Count - i));
                }
                else if (cmd[0].Equals("cls"))
                    textBox6.Clear();
                else
                    WriteOutput("Invalid command - type \"help\" for a list of commands");

				textBox5.Clear();
                clickpanel.Invalidate();
			} //if input textbox not empty
		}

        //up or down key for prev commands
        void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                if (prevCommandIndex > 0 && e.KeyCode == Keys.Down)
                    prevCommandIndex--;

                if (prevCommandIndex < prevCommands.Count && e.KeyCode == Keys.Up)
                    prevCommandIndex++;

                if (prevCommandIndex == 0)
                    textBox5.Text = "";
                else
                    textBox5.Text = prevCommands.ElementAt(prevCommands.Count - prevCommandIndex);

                e.Handled = true;
            }
        }

		//open graph dialog
		private void button1_Click(object sender, EventArgs e)
		{
			Stream myStream = null;
			OpenFileDialog openFileDialog1 = new OpenFileDialog();

			openFileDialog1.InitialDirectory = launchPath + "\\graphs";
			openFileDialog1.Filter = "Node Graphs (*.wmap)|*.wmap";
			openFileDialog1.FilterIndex = 2;
			openFileDialog1.RestoreDirectory = true;

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				try
				{
					if ((myStream = openFileDialog1.OpenFile()) != null)
					{
						using (myStream)
						{
                            InterruptThreads();
							BinaryFormatter bFormatter = new BinaryFormatter();
							graph = (Graph) bFormatter.Deserialize(myStream);
                            graph.Modified += new Graph.GraphModifiedHandler(graph_Modified);
                            nav.SetNewGraphReference(graph);
							myStream.Close();

                            this.Text = "GuiltySpark - " + openFileDialog1.FileName;
                            clickpanel.Invalidate();
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
				}
			}
		}

        public void OpenFile(String filename)
        {
            try
			{
                Stream myStream = new FileStream(filename, FileMode.Open);

                InterruptThreads();
                BinaryFormatter bFormatter = new BinaryFormatter();
                graph = (Graph)bFormatter.Deserialize(myStream);
                graph.Modified += new Graph.GraphModifiedHandler(graph_Modified);
                nav.SetNewGraphReference(graph);
                myStream.Close();

                this.Text = "GuiltySpark - " + filename;
                clickpanel.Invalidate();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        //save graph dialog
		private void button2_Click(object sender, EventArgs e)
		{
			Stream myStream;
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();

			saveFileDialog1.Filter = "waypoint maps (*.wmap)|*.wmap"; ;
			saveFileDialog1.FilterIndex = 2;
			saveFileDialog1.RestoreDirectory = true;

			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				if ((myStream = saveFileDialog1.OpenFile()) != null)
				{
                    InterruptThreads();
					BinaryFormatter bFormatter = new BinaryFormatter();
					bFormatter.Serialize(myStream, graph);
					myStream.Close();
				}
			}

            this.Text = "GuiltySpark - " + saveFileDialog1.FileName;
		}

        //send changed node radius to navigation
		private void nodeRadiusSelector_ValueChanged(object sender, EventArgs e)
		{
			nav.NodeRadius = (float)nodeRadiusSelector.Value;
		}

        //stops some program acitivity
		private void InterruptThreads()
		{
            ai.Stop();                          //needed?
            if (nav != null)
                nav.Stop();
            clickpanel.Invalidate();
		}

		//write a line to the command output window
		private void WriteOutput(string s)
		{
            if (textBox6.Text.Length + s.Length > textBox6.MaxLength)
                textBox6.Text = "";
			textBox6.AppendText(s + "\r\n");
            SendMessage(textBox6.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
		}

        public delegate void ToggleAIButtonsDelegate(bool on);

        public void ToggleAIButtons(bool on)
        {
            startAIButton.Enabled = !on;
            stopAIButton.Enabled = on;
        }

        public delegate void WriteAIDelegate(String str);

        private const int WM_VSCROLL = 0x115;
        private const int SB_BOTTOM = 7;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);
        
        //write a line to the AI output window
        public void WriteAI(string s)
        {
            if (aiTextBox.Text.Length + s.Length > aiTextBox.MaxLength)
                aiTextBox.Text = "";
            aiTextBox.AppendText(s + "\r\n");
            SendMessage(aiTextBox.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
        }

        private void lookAheadCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            nav.AimAhead = lookAheadCheckBox.Checked;
            clickpanel.Invalidate();
        }

        //ai interval
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ai.ChangeInterval((int)aiIntervalSelector.Value);
        }

        private void startAIButton_Click(object sender, EventArgs e)
        {
            ai.Start();
        }

        private void stopAIButton_Click(object sender, EventArgs e)
        {
            ai.Stop();
        }

        private void loadAIButton_Click(object sender, EventArgs e)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = launchPath + "\\ai";
            openFileDialog1.Filter = "AI Scripts (*.txt)|*.txt";
            openFileDialog1.FilterIndex = 2;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            ai.Load(myStream);
                            myStream.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            ai.DebugPrinting = checkBox6.Checked;
        }

        //clear WriteAI box
        private void button3_Click(object sender, EventArgs e)
        {
            aiTextBox.Clear();
        }

        //use camera
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void graphScaleSelector_ValueChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void zoomSelector_ValueChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void horizontalAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            //dxpanel.Invalidate();
        }

        private void verticalAngleUpDown_ValueChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void autoRotateCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            clickpanel.Invalidate();
        }

        public void setAIInterval(int t)
        {
            if (t >= aiIntervalSelector.Minimum && t <= aiIntervalSelector.Maximum)
                aiIntervalSelector.Value = t;
        }
	}  
}