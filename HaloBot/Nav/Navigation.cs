using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace HaloBot
{
    public class Navigation
    {
        private const float STRAFE_SCALE = 2.5F;

        private Graph graph;
        private MemoryReaderWriter gameState;
        private Thread walker;

        public bool StrafeMode;
        public bool AimAhead;
        public Aimbot aimbot;

        public bool keepChatting;
        private volatile bool stopRequested;
        public volatile float NodeRadius;
        public volatile Stack<Waypoint> CurrentPath;
        public volatile bool Walking;

        //class constructor
        public Navigation(Graph graph, MemoryReaderWriter gameState)
        {
            CurrentPath = new Stack<Waypoint>();
            walker = new Thread(new ThreadStart(WalkPath));
            walker.Name = "Walker";
            walker.IsBackground = true;
            this.graph = graph;
            this.gameState = gameState;

            aimbot = new Aimbot(gameState);

            Walking = false;
            StrafeMode = false;
            stopRequested = true;
            NodeRadius = 0.5F;
            walker.Start();
            
        }

        #region Control
        //finds a path and walks along it
        public void WalkTo(ushort dst, bool alternate)
        {
            if (FindPath(dst, alternate))
                stopRequested = false;
            else
                stopRequested = true;
        }

        public void WalkTo(Structures.FLOAT3 end, bool alternate)
        {
            ushort dst1 = GetClosestWaypoint(end);
            
            if (FindPath(dst1, alternate))
            {
                stopRequested = false;
                
                Stack<Waypoint> temp = new Stack<Waypoint>();
                Waypoint[] temp2 = CurrentPath.ToArray();
                temp.Push(new Waypoint(end));
                for (int i = temp2.Length - 1; i >=0; i--)
                    temp.Push(temp2[i]);
                CurrentPath = temp;
            }
            else
                stopRequested = true;
        }
        
        //can be used to manually stop the walker
        public void Stop()
        {
            aimbot.LookAheadUnlock();
            aimbot.NavUnlock();
            stopRequested = true;
            Walking = false;
            keepChatting = false;
            //CurrentPath.Clear();
        }

        public void SetNewGraphReference(Graph graph)
        {
            this.graph = graph;
        }
        #endregion

        #region Pathfinding
        public ushort GetClosestWaypoint(Structures.FLOAT3 pos)
        {
            float shortestDistance = float.PositiveInfinity;
            ushort closestWaypointIndex = 0;

            for (ushort i = 1; i <= graph.LastIndex; i++)
                if (graph.pool[i] != null)
                {
                    float tentativeDistance = Distance3D(graph.pool[i].pos.X, graph.pool[i].pos.Y, graph.pool[i].pos.Z,
                        pos.X, pos.Y, pos.Z);
                    if (tentativeDistance < shortestDistance)
                    {
                        shortestDistance = tentativeDistance;
                        closestWaypointIndex = i;
                    }
                }
            return closestWaypointIndex;
        }

        //A* pathfinding algorithm
        private bool FindPath(ushort goal, bool alternate)
        {
            graph.ResetCosts();

            ushort[] closedSet = new ushort[1000];
            ushort[] openSet = new ushort[1000];
            int closedSetCount = 0;
            int openSetCount = 1;

            openSet[0] = GetClosestWaypoint(gameState.LocalPosition);

            graph.pool[openSet[0]].CostG = 0;
            graph.pool[openSet[0]].CostH = Distance3D(0, goal);

            while (openSetCount > 0 && openSetCount < 1000)
            {
                ushort current = PopLowestF(openSet, openSetCount);
                openSetCount--;

                if (current == goal)
                {
                    //Debug.WriteLine("pathfinding success");
                    ReconstructPath(current);
                    return true;
                }
                
                closedSet[closedSetCount++] = current;

                //for each surrounding (linked) node of current
                for (byte s = 0; s < graph.pool[current].NumberOfConnections; s++)
                {
                    ushort surroundingIndex = graph.pool[current].SurroundingIndexes[s];

                    //if current's SurroundingIndexes[s] is in the closedSet, skip to next s
                    for (int check = 0; check < closedSetCount; check++)
                        if (closedSet[check] == surroundingIndex)
                            continue;

                    float tentativeGScore = graph.pool[current].CostG +
                        ((graph.pool[current].ConnectionTypes[s] == 2) ? 0 : Distance3D(current, surroundingIndex)) +
                        ((alternate) ? graph.pool[current].PersistentCost : 0);

                    bool tentativeIsBetter = false;
                    if (graph.pool[surroundingIndex].CostG == float.PositiveInfinity)
                    {
                        //add surrounding node to the open set
                        openSet[openSetCount++] = surroundingIndex;
                        tentativeIsBetter = true;
                    }
                    else if (tentativeGScore < graph.pool[surroundingIndex].CostG)
                        tentativeIsBetter = true;

                    if (tentativeIsBetter)
                    {
                        graph.pool[surroundingIndex].Parent = current;
                        graph.pool[surroundingIndex].CostG = tentativeGScore;
                        graph.pool[surroundingIndex].CostH = Distance3D(surroundingIndex, goal);
                    }

                } //end for each surrounding
            } //end while openListCount > 0

            Debug.WriteLine("pathfinding failed");
            //failed
            return false;
        }

        //reconstructs the path found by FindPath() for use by walker
        private void ReconstructPath(ushort goal)
        {
            Waypoint p = graph.pool[goal];
            CurrentPath.Clear();
            while (p.Parent != 0)
            {
                p.PersistentCost += 1.0F;
                CurrentPath.Push(p);
                p = graph.pool[p.Parent];
            }
            CurrentPath.Push(p);
        }

        //pops the lowest F-Score node from the open set priority queue
        private ushort PopLowestF(ushort[] openSet, int openSetCount)
        {
            int index = 0;
            float lowestCost = graph.pool[openSet[0]].GetFCost();
            for (int i = 0; i < openSetCount; i++)
            {
                float tentativeCost = graph.pool[openSet[i]].GetFCost();
                if (tentativeCost < lowestCost)
                {
                    lowestCost = tentativeCost;
                    index = i;
                }
            }

            ushort lowestFScoreIndex = openSet[index];

            for (int j = index; j < openSetCount - 1; j++)
                openSet[j] = openSet[j + 1];

            return lowestFScoreIndex;
        }

        //returns the 3D distance between two nodes given by their pool index
        private float Distance3D(ushort start, ushort goal)
        {
            return (float)Math.Sqrt(Math.Pow(graph.pool[start].pos.X - graph.pool[goal].pos.X, 2) +
                                    Math.Pow(graph.pool[start].pos.Y - graph.pool[goal].pos.Y, 2) +
                                    Math.Pow(graph.pool[start].pos.Z - graph.pool[goal].pos.Z, 2));
        }

        public String GetWindow()
        {
            StringBuilder sb = new StringBuilder();
            IntPtr foregroundHandle = GetForegroundWindow();
            sb.Remove(0, sb.ToString().Length);
            sb.Capacity = GetWindowTextLength(foregroundHandle) + 1;
            GetWindowText(foregroundHandle, sb, sb.Capacity);
            return sb.ToString();
        }

        #endregion

        #region Walking

        private void WalkPath()
        {
            while (true)
            {
                Thread.Sleep(150);
                

            stop_walking:
                Walking = false;
                aimbot.NavUnlock();

                if (!stopRequested)
                {
                    Walking = true;
                    Waypoint cameFrom = graph.pool[0];

                    //continue while there are more nodes to go to
                    while (CurrentPath.Count != 0)
                    {
                        Waypoint nextGoal = CurrentPath.Peek();
                        if (nextGoal == null)
                                break;

                        byte nextType = graph.pool[nextGoal.Parent].GetLinkType(nextGoal, graph.pool);
                        Structures.FLOAT3 currentPosition = gameState.LocalPosition;
                        Structures.FLOAT3 nextPosition = nextGoal.pos;

                        float xOffset = nextPosition.X - currentPosition.X;
                        float yOffset = nextPosition.Y - currentPosition.Y;
                        float zOffset = nextPosition.Z - currentPosition.Z;
                        float distToGoal;
                        float initialDist = Distance3D(nextPosition.X, nextPosition.Y, nextPosition.Z,
                            currentPosition.X, currentPosition.Y, currentPosition.Z);
                        double initialRatio = xOffset / yOffset;
                        float playerVelocity = 0;

                        aimbot.NavUnlock();

                        if (nextType == 5 || nextType == 3)
                        {
                            aimbot.NavLock(nextPosition);
                        }

                        //while not at nextGoal
                        while ((distToGoal = Distance3D(nextPosition.X, nextPosition.Y, nextPosition.Z,
                            currentPosition.X, currentPosition.Y, currentPosition.Z)) > NodeRadius)
                        {
                            if (stopRequested)
                            {
                                CurrentPath.Clear();
                                goto stop_walking;
                            }

                            //if the path gets changed by another thread, get out
                            if (!CurrentPath.Contains(nextGoal))
                            {
                                break;
                            }

                            double angleDifference = Math.Atan2(yOffset, xOffset) - gameState.PlayerHorizontalViewAngle;
                            if (angleDifference < -Math.PI)
                                angleDifference += 2 * Math.PI;
                            else if (angleDifference > Math.PI)
                                angleDifference -= 2 * Math.PI;
                            double myRatio = xOffset / yOffset;
                            ushort dominantKey = (ushort)DIK.DIK_W;
                            ushort secondaryKey = (ushort)DIK.DIK_W;
                            ushort modifierKey = 0;
                            bool over = false;

                            #region calculate_keys

                            if (nextType == 3 || (playerVelocity < 0.1F && Math.Abs(initialDist - distToGoal) > 0.5F
                                && nextType != 4))
                                modifierKey = (ushort)DIK.DIK_SPACE;
                            else if (nextType == 4)
                                modifierKey = (ushort)DIK.DIK_LCONTROL;

                            if (angleDifference > 0)
                            {
                                if (angleDifference < Math.PI / 4)
                                {
                                    dominantKey = (ushort)DIK.DIK_W;
                                    secondaryKey = (ushort)DIK.DIK_A;
                                }
                                else if (angleDifference < Math.PI / 2)
                                {
                                    dominantKey = (ushort)DIK.DIK_A;
                                    secondaryKey = (ushort)DIK.DIK_W;
                                    over = true;
                                }
                                else if (angleDifference < (3 * Math.PI) / 4)
                                {
                                    dominantKey = (ushort)DIK.DIK_A;
                                    secondaryKey = (ushort)DIK.DIK_S;
                                }
                                else if (angleDifference < Math.PI)
                                {
                                    dominantKey = (ushort)DIK.DIK_S;
                                    secondaryKey = (ushort)DIK.DIK_A;
                                    over = true;
                                }
                            }
                            else if (angleDifference < 0)
                            {
                                if (angleDifference > -Math.PI / 4)
                                {
                                    dominantKey = (ushort)DIK.DIK_W;
                                    secondaryKey = (ushort)DIK.DIK_D;
                                    over = true;
                                }
                                else if (angleDifference > -Math.PI / 2)
                                {
                                    dominantKey = (ushort)DIK.DIK_D;
                                    secondaryKey = (ushort)DIK.DIK_W;
                                }
                                else if (angleDifference > -(3 * Math.PI) / 4)
                                {
                                    dominantKey = (ushort)DIK.DIK_D;
                                    secondaryKey = (ushort)DIK.DIK_S;
                                    over = true;
                                }
                                else if (angleDifference > -Math.PI)
                                {
                                    dominantKey = (ushort)DIK.DIK_S;
                                    secondaryKey = (ushort)DIK.DIK_D;
                                }
                            }
                            #endregion

                            if (AimAhead)
                                aimbot.LookAheadLock(nextPosition);
                            else
                                aimbot.LookAheadUnlock();

                            if (nextType == 5 || nextType == 3 || AimAhead)
                                secondaryKey = (ushort)DIK.DIK_W;

                            #region SendInput
                            if (modifierKey != 0 && distToGoal > 1)
                                Press(modifierKey, -1, true);
                            Press(dominantKey, -1, true);

                            if ((myRatio > initialRatio && over) || (myRatio < initialRatio && !over))
                            {
                                Press(secondaryKey, 100, true);
                            }
                            else
                                Thread.Sleep(100);

                            Press(dominantKey, 0, true);
                            Press(modifierKey, 0, true);
                            #endregion SendInput

                            currentPosition = gameState.LocalPosition;

                            //can modify positions if we don't want it to follow the path exactly
                            if (StrafeMode && nextType != 5 && nextType != 3)
                            {
                                Structures.FLOAT3 randomOffset = new Structures.FLOAT3((float)aimbot.rand.NextDouble() - 0.5F,
                                                                                       (float)aimbot.rand.NextDouble() - 0.5F,
                                                                                       (float)aimbot.rand.NextDouble() - 0.5F);
                                currentPosition += (randomOffset * STRAFE_SCALE);
                            }

                            playerVelocity = Distance3D(xOffset, yOffset, zOffset,
                                xOffset = nextPosition.X - currentPosition.X,
                                yOffset = nextPosition.Y - currentPosition.Y,
                                zOffset = nextPosition.Z - currentPosition.Z);

                        } //end while not at next goal

                        //path might get cleared by another thread and we didnt catch it earlier
                        if (CurrentPath.Count != 0)
                            CurrentPath.Pop();
                        cameFrom = graph.pool[0];
                    }

                    stopRequested = true;
                } //end if (Walking)
            } //end while (true)
        }

        //returns the 3D distance between two points given by a pair of XYZ coordinates
        public static float Distance3D(float x1, float y1, float z1, float x, float y, float z)
        {
            return (float)Math.Sqrt(Math.Pow(x1 - x, 2) +
                                    Math.Pow(y1 - y, 2) +
                                    Math.Pow(z1 - z, 2));
        }

        public static float Distance3D(Structures.FLOAT3 p0, Structures.FLOAT3 p1)
        {
            return Distance3D(p0.X, p0.Y, p0.Z, p1.X, p1.Y, p1.Z);
        }

        //returns the 2D distance between two points given by a pair of offsets
        public static float Distance2D(float dx, float dy)
        {
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        #endregion Walking

        #region key_input

        //direct input key codes from http://community.bistudio.com/wiki/DIK_KeyCodes
        public enum DIK : ushort
        {
            DIK_ESCAPE = 0x01
                ,
            DIK_1 = 0x02
                ,
            DIK_2 = 0x03
                ,
            DIK_3 = 0x04
                ,
            DIK_4 = 0x05
                ,
            DIK_5 = 0x06
                ,
            DIK_6 = 0x07
                ,
            DIK_7 = 0x08
                ,
            DIK_8 = 0x09
                ,
            DIK_9 = 0x0A
                ,
            DIK_0 = 0x0B
                ,
            DIK_MINUS = 0x0C/*-onmainkeyboard*/
                ,
            DIK_EQUALS = 0x0D
                ,
            DIK_BACK = 0x0E/*backspace*/
                ,
            DIK_TAB = 0x0F
                ,
            DIK_Q = 0x10
                ,
            DIK_W = 0x11
                ,
            DIK_E = 0x12
                ,
            DIK_R = 0x13
                ,
            DIK_T = 0x14
                ,
            DIK_Y = 0x15
                ,
            DIK_U = 0x16
                ,
            DIK_I = 0x17
                ,
            DIK_O = 0x18
                ,
            DIK_P = 0x19
                ,
            DIK_LBRACKET = 0x1A
                ,
            DIK_RBRACKET = 0x1B
                ,
            DIK_RETURN = 0x1C/*Enteronmainkeyboard*/
                ,
            DIK_LCONTROL = 0x1D
                ,
            DIK_A = 0x1E
                ,
            DIK_S = 0x1F
                ,
            DIK_D = 0x20
                ,
            DIK_F = 0x21
                ,
            DIK_G = 0x22
                ,
            DIK_H = 0x23
                ,
            DIK_J = 0x24
                ,
            DIK_K = 0x25
                ,
            DIK_L = 0x26
                ,
            DIK_SEMICOLON = 0x27
                ,
            DIK_APOSTROPHE = 0x28
                ,
            DIK_GRAVE = 0x29/*accentgrave*/
                ,
            DIK_LSHIFT = 0x2A
                ,
            DIK_BACKSLASH = 0x2B
                ,
            DIK_Z = 0x2C
                ,
            DIK_X = 0x2D
                ,
            DIK_C = 0x2E
                ,
            DIK_V = 0x2F
                ,
            DIK_B = 0x30
                ,
            DIK_N = 0x31
                ,
            DIK_M = 0x32
                ,
            DIK_COMMA = 0x33
                ,
            DIK_PERIOD = 0x34/*.onmainkeyboard*/
                ,
            DIK_SLASH = 0x35/*/onmainkeyboard*/
                ,
            DIK_RSHIFT = 0x36
                ,
            DIK_MULTIPLY = 0x37/**onnumerickeypad*/
                ,
            DIK_LMENU = 0x38/*leftAlt*/
                ,
            DIK_SPACE = 0x39
                ,
            DIK_CAPITAL = 0x3A
                ,
            DIK_F1 = 0x3B
                ,
            DIK_F2 = 0x3C
                ,
            DIK_F3 = 0x3D
                ,
            DIK_F4 = 0x3E
                ,
            DIK_F5 = 0x3F
                ,
            DIK_F6 = 0x40
                ,
            DIK_F7 = 0x41
                ,
            DIK_F8 = 0x42
                ,
            DIK_F9 = 0x43
                ,
            DIK_F10 = 0x44
                ,
            DIK_NUMLOCK = 0x45
                ,
            DIK_SCROLL = 0x46/*ScrollLock*/
                ,
            DIK_NUMPAD7 = 0x47
                ,
            DIK_NUMPAD8 = 0x48
                ,
            DIK_NUMPAD9 = 0x49
                ,
            DIK_SUBTRACT = 0x4A/*-onnumerickeypad*/
                ,
            DIK_NUMPAD4 = 0x4B
                ,
            DIK_NUMPAD5 = 0x4C
                ,
            DIK_NUMPAD6 = 0x4D
                ,
            DIK_ADD = 0x4E/*+onnumerickeypad*/
                ,
            DIK_NUMPAD1 = 0x4F
                ,
            DIK_NUMPAD2 = 0x50
                ,
            DIK_NUMPAD3 = 0x51
                ,
            DIK_NUMPAD0 = 0x52
                ,
            DIK_DECIMAL = 0x53/*.onnumerickeypad*/
                ,
            DIK_OEM_102 = 0x56/*<>|onUK/Germanykeyboards*/
                ,
            DIK_F11 = 0x57
                ,
            DIK_F12 = 0x58
                ,
            DIK_F13 = 0x64/*(NECPC98)*/
                ,
            DIK_F14 = 0x65/*(NECPC98)*/
                ,
            DIK_F15 = 0x66/*(NECPC98)*/
                ,
            DIK_KANA = 0x70/*(Japanesekeyboard)*/
                ,
            DIK_ABNT_C1 = 0x73/*/?onPortugese(Brazilian)keyboards*/
                ,
            DIK_CONVERT = 0x79/*(Japanesekeyboard)*/
                ,
            DIK_NOCONVERT = 0x7B/*(Japanesekeyboard)*/
                ,
            DIK_YEN = 0x7D/*(Japanesekeyboard)*/
                ,
            DIK_ABNT_C2 = 0x7E/*Numpad.onPortugese(Brazilian)keyboards*/
                ,
            DIK_NUMPADEQUALS = 0x8D/*=onnumerickeypad(NECPC98)*/
                ,
            DIK_PREVTRACK = 0x90/*PreviousTrack(,DIK_CIRCUMFLEXonJapanesekeyboard)*/
                ,
            DIK_AT = 0x91/*(NECPC98)*/
                ,
            DIK_COLON = 0x92/*(NECPC98)*/
                ,
            DIK_UNDERLINE = 0x93/*(NECPC98)*/
                ,
            DIK_KANJI = 0x94/*(Japanesekeyboard)*/
                ,
            DIK_STOP = 0x95/*(NECPC98)*/
                ,
            DIK_AX = 0x96/*(JapanAX)*/
                ,
            DIK_UNLABELED = 0x97/*(J3100)*/
                ,
            DIK_NEXTTRACK = 0x99/*NextTrack*/
                ,
            DIK_NUMPADENTER = 0x9C/*Enteronnumerickeypad*/
                ,
            DIK_RCONTROL = 0x9D
                ,
            DIK_MUTE = 0xA0/*Mute*/
                ,
            DIK_CALCULATOR = 0xA1/*Calculator*/
                ,
            DIK_PLAYPAUSE = 0xA2/*Play/Pause*/
                ,
            DIK_MEDIASTOP = 0xA4/*MediaStop*/
                ,
            DIK_VOLUMEDOWN = 0xAE/*Volume-*/
                ,
            DIK_VOLUMEUP = 0xB0/*Volume+*/
                ,
            DIK_WEBHOME = 0xB2/*Webhome*/
                ,
            DIK_NUMPADCOMMA = 0xB3/*,onnumerickeypad(NECPC98)*/
                ,
            DIK_DIVIDE = 0xB5/*/onnumerickeypad*/
                ,
            DIK_SYSRQ = 0xB7
                ,
            DIK_RMENU = 0xB8/*rightAlt*/
                ,
            DIK_PAUSE = 0xC5/*Pause*/
                ,
            DIK_HOME = 0xC7/*Homeonarrowkeypad*/
                ,
            DIK_UP = 0xC8/*UpArrowonarrowkeypad*/
                ,
            DIK_PRIOR = 0xC9/*PgUponarrowkeypad*/
                ,
            DIK_LEFT = 0xCB/*LeftArrowonarrowkeypad*/
                ,
            DIK_RIGHT = 0xCD/*RightArrowonarrowkeypad*/
                ,
            DIK_END = 0xCF/*Endonarrowkeypad*/
                ,
            DIK_DOWN = 0xD0/*DownArrowonarrowkeypad*/
                ,
            DIK_NEXT = 0xD1/*PgDnonarrowkeypad*/
                ,
            DIK_INSERT = 0xD2/*Insertonarrowkeypad*/
                ,
            DIK_DELETE = 0xD3/*Deleteonarrowkeypad*/
                ,
            DIK_LWIN = 0xDB/*LeftWindowskey*/
                ,
            DIK_RWIN = 0xDC/*RightWindowskey*/
                ,
            DIK_APPS = 0xDD/*AppMenukey*/
                ,
            DIK_POWER = 0xDE/*SystemPower*/
                ,
            DIK_SLEEP = 0xDF/*SystemSleep*/
                ,
            DIK_WAKE = 0xE3/*SystemWake*/
                ,
            DIK_WEBSEARCH = 0xE5/*WebSearch*/
                ,
            DIK_WEBFAVORITES = 0xE6/*WebFavorites*/
                ,
            DIK_WEBREFRESH = 0xE7/*WebRefresh*/
                ,
            DIK_WEBSTOP = 0xE8/*WebStop*/
                ,
            DIK_WEBFORWARD = 0xE9/*WebForward*/
                ,
            DIK_WEBBACK = 0xEA/*WebBack*/
                ,
            DIK_MYCOMPUTER = 0xEB/*MyComputer*/
                ,
            DIK_MAIL = 0xEC/*Mail*/
                ,
            DIK_MEDIASELECT = 0xED/*MediaSelect*/
        }

        public enum KEY_EVENTS : int
        {
            INPUT_MOUSE = 0,
            INPUT_KEYBOARD = 1,
            INPUT_HARDWARE = 2,
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_UNICODE = 0x0004,
            KEYEVENTF_SCANCODE = 0x0008,
            XBUTTON1 = 0x0001,
            XBUTTON2 = 0x0002,
            MOUSEEVENTF_MOVE = 0x0001,
            MOUSEEVENTF_LEFTDOWN = 0x0002,
            MOUSEEVENTF_LEFTUP = 0x0004,
            MOUSEEVENTF_RIGHTDOWN = 0x0008,
            MOUSEEVENTF_RIGHTUP = 0x0010,
            MOUSEEVENTF_MIDDLEDOWN = 0x0020,
            MOUSEEVENTF_MIDDLEUP = 0x0040,
            MOUSEEVENTF_XDOWN = 0x0080,
            MOUSEEVENTF_XUP = 0x0100,
            MOUSEEVENTF_WHEEL = 0x0800,
            MOUSEEVENTF_VIRTUALDESK = 0x4000,
            MOUSEEVENTF_ABSOLUTE = 0x8000
        }


        #region input structures
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT
        {
            [FieldOffset(0)]
            public IntPtr type;
            [FieldOffset(4)]
            public MOUSEINPUT mi;
            [FieldOffset(4)]
            public KEYBDINPUT ki;
            [FieldOffset(4)]
            public HARDWAREINPUT hi;
        }
        #endregion

        [DllImport("User32.dll")]
        private static extern uint SendInput(uint nInputs, ref INPUT pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();


        public void Click(bool mouse1, int time)
        {
            if (GetWindow().Equals("Halo") && gameState.CanWalk)
            {
                INPUT dominantInput = new INPUT();
                dominantInput.type = (IntPtr)KEY_EVENTS.INPUT_MOUSE;
                dominantInput.mi.mouseData = 0;
                dominantInput.mi.dwExtraInfo = (IntPtr)0;
                dominantInput.mi.time = 0;

                //mouse down
                if (time != 0)
                {
                    dominantInput.mi.dwFlags = mouse1 ? (uint)KEY_EVENTS.MOUSEEVENTF_LEFTDOWN :
                                                             (uint)KEY_EVENTS.MOUSEEVENTF_RIGHTDOWN;
                    SendInput(1, ref dominantInput, Marshal.SizeOf(typeof(INPUT)));

                    if (time == -1)
                        return;
                }

                if (time > 0)
                    System.Threading.Thread.Sleep(time);

                //mouse up
                dominantInput.mi.dwFlags = mouse1 ? (uint)KEY_EVENTS.MOUSEEVENTF_LEFTUP :
                                                         (uint)KEY_EVENTS.MOUSEEVENTF_RIGHTUP;
                SendInput(1, ref dominantInput, Marshal.SizeOf(typeof(INPUT)));
            }
        }

        //-1 keydown, 0 keyup, else sleep between
        public void Press(ushort keycode, int time, bool safe)
        {
            if (GetWindow().Equals("Halo") && (safe ? gameState.CanWalk : true))
            {               
                INPUT keyInput = new INPUT();
                keyInput.type = (IntPtr)KEY_EVENTS.INPUT_KEYBOARD;
                keyInput.ki.time = 0;
                keyInput.ki.wScan = keycode;
                
                

                if (time != 0)
                {
                    keyInput.ki.dwFlags = (int)KEY_EVENTS.KEYEVENTF_SCANCODE;
                    SendInput(1, ref keyInput, Marshal.SizeOf(typeof(INPUT)));

                    

                    if (time == -1)
                        return;
                }
                
                if (time > 0)
                    System.Threading.Thread.Sleep(time);

                //keyup
                keyInput.ki.dwFlags = (int)KEY_EVENTS.KEYEVENTF_SCANCODE | (int)KEY_EVENTS.KEYEVENTF_KEYUP;
                SendInput(1, ref keyInput, Marshal.SizeOf(typeof(INPUT)));
            }
        }


        [DllImport("user32.dll")]
        static extern ushort VkKeyScan(char ch);

        [DllImport("user32.dll")]
        static extern ushort MapVirtualKey(ushort uCode, uint uMapType);

        public void PressChar(char c)
        {
            switch (c)
            {
                case '�':
                    Press((ushort)DIK.DIK_LMENU, -1, false);
                    Press((ushort)DIK.DIK_NUMPAD2, 1, false);
                    Press((ushort)DIK.DIK_NUMPAD5, 1, false);
                    Press((ushort)DIK.DIK_NUMPAD5, 1, false);
                    Press((ushort)DIK.DIK_LMENU, 0, false);
                    break;
                case '$':
                    Thread.Sleep(1000);
                    break;
                case '#':
                    Press((ushort)DIK.DIK_LSHIFT, -1, false);
                    PressChar('3');
                    Press((ushort)DIK.DIK_LSHIFT, 0, false);
                    break;
                case '!':
                    Press((ushort)DIK.DIK_LSHIFT, -1, false);
                    PressChar('1');
                    Press((ushort)DIK.DIK_LSHIFT, 0, false);
                    break;
                case '(':
                    Press((ushort)DIK.DIK_LSHIFT, -1, false);
                    PressChar('9');
                    Press((ushort)DIK.DIK_LSHIFT, 0, false);
                    break;
                case ')':
                    Press((ushort)DIK.DIK_LSHIFT, -1, false);
                    PressChar('0');
                    Press((ushort)DIK.DIK_LSHIFT, 0, false);
                    break;
                case '|':
                    Press((ushort)DIK.DIK_LSHIFT, -1, false);
                    PressChar('\\');
                    Press((ushort)DIK.DIK_LSHIFT, 0, false);
                    break;
                case '_':
                    Press((ushort)DIK.DIK_LSHIFT, -1, false);
                    PressChar('-');
                    Press((ushort)DIK.DIK_LSHIFT, 0, false);
                    break;
                case '@':
                    Press((ushort)DIK.DIK_LSHIFT, -1, false);
                    PressChar('2');
                    Press((ushort)DIK.DIK_LSHIFT, 0, false);
                    break;
                default:
                    ushort vkey = VkKeyScan(c);
                    ushort key = MapVirtualKey(vkey, 0);
                    Press(key, 1, false);
                    break;
            }

            Thread.Sleep(5);
        }


        public bool Chat(String filename)
        {
            try
            {
                keepChatting = true;
                Stream fs = new FileStream(filename, FileMode.Open);
                using (fs)
                {
                    StreamReader sr = new StreamReader(fs);
                    String line;
                    while ((line = sr.ReadLine()) != null && keepChatting)
                    {
                        Press((ushort)DIK.DIK_T, 35, true);
                        foreach (char c in line)
                        {
                            PressChar(c);
                        }
                        Press((ushort)DIK.DIK_RETURN, 35, false);
                    }


                    fs.Close();
                }
                return true;

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return false;
            }
        }

        #endregion
    }
}