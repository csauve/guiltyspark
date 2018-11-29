using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace HaloBot
{
    public class AIHandler
    {
        public enum FID : int
        {
            PRINT = 0,
            SET_TARGET_PLAYER = 1,
            GOTO_NODE = 2,
            GOTO_PLAYER = 3,
            GOTO_NODE_ALT = 4,
            GOTO_PLAYER_ALT = 5,
            MOUSE1 = 6,
            MOUSE2 = 7,
            SLEEP = 8,
            SET_VALUE1 = 9,
            SET_VALUE2 = 10,
            TOGGLE_AIMBOT = 11,
            //reserved
            SET_TARGET_NODE = 13,
            SET_LOOK_AHEAD_MODE = 14,
            //reserved
            SET_VALUE3 = 16,
            SET_VALUE4 = 17,
            SET_VALUE5 = 18,
            AIMBOT_ENABLE_ARC_MODE = 19,
            AIMBOT_DISABLE_ARC_MODE = 20,
            SET_PROJECTILE_VELOCITY = 21,
            GOTO_OBJECTIVE = 22,
            GOTO_OBJECTIVE_ALT = 23,
            SET_STRAFE_MODE = 24,

            CROUCH = 25,
            JUMP = 26,
            SWITCH_WEAPONS = 27,
            MELEE = 28,
            RELOAD = 29,
            ZOOM = 30,
            FLASHLIGHT = 31,

            SET_PRIMARY_WEAPON = 32,
            BACKPACK_RELOAD = 33,
            EXCHANGE_WEAPON = 34,
            ACTION = 35,
            SWITCH_GRENADE_TYPE = 36,
            
            CHAT = 37
        }

        public enum DATA_SOURCES : int
        {
            PLAYER_X = 0,
            PLAYER_Y = 1,
            PLAYER_Z = 2,
            CAN_WALK = 3,
            VIEW_ANGLE_H = 4,
            VIEW_ANGLE_V = 5,
            CAMERA_X = 6,
            CAMERA_Y = 7,
            CAMERA_Z = 8,
            CLOSEST_ENEMY = 9,
            LOCAL_TEAM = 10,
            PLAYER_COUNT = 11,
            LOCAL_PING = 12,
            CLEAR_SHOT = 13,
            SHIFT_KEY = 14,
            VALUE1 = 15,
            VALUE2 = 16,
            CLOSEST_ALLY = 17,
            CLOSEST_ANYONE = 18,
            CLOSEST_DIST = 19,
            TARGET_HEALTH = 20,
            LOCAL_HEALTH = 21,
            TARGET_DIST = 22,
            TARGET_SHIELD = 23,
            LOCAL_SHIELD = 24,
            TARGET_CAMO = 25,
            LOCAL_CAMO = 26,
            TARGET_STANCE = 27,
            LOCAL_STANCE = 28,
            ENEMY_NEAREST_VIEW = 29,
            ALLY_NEAREST_VIEW = 30,
            ANYONE_NEAREST_VIEW = 31,
            VALUE3 = 32,
            VALUE4 = 33,
            VALUE5 = 34,
            RANDOM_NODE = 35,
            ZOOM_LEVEL = 36,
            FLASHLIGHT = 37,

            //weapon
            PRIMARY_WEAPON = 38,
            WEAPON0_TYPE = 39,
            WEAPON1_TYPE = 40,
            WEAPON0_CLIP = 41,
            WEAPON1_CLIP = 42,
            WEAPON0_RESERVE = 43,
            WEAPON1_RESERVE = 44,

            //grenades
            GRENADE_TYPE = 45,
            FRAG_GRENADE_COUNT = 46,
            PLASMA_GRENADE_COUNT = 47,

            RANDOM = 48,

            CLEAR_SHOT_ADVANCED = 49        
        }

        public static int MAX_STRING_BUFFER_SIZE = 200;

        public Form1 form1;
        public MemoryReaderWriter gameState;
        Subtask root;
        private Timer aiTimer;
        private bool ready;
        private bool aiIsRunning;
        public bool DebugPrinting;

        //use this random number when accessing data sources to see if it's already been
        //accessed during the current tick
        private Random r;
        public int random;
        private int[] sourceTouches = new int[Enum.GetValues(typeof(DATA_SOURCES)).Length];
        private double[] dataSources = new double[Enum.GetValues(typeof(DATA_SOURCES)).Length];

        private Form1.WriteAIDelegate AIOut;
        private Form1.ToggleAIButtonsDelegate ToggleDel;

        public AIHandler(Form1 form, Form1.WriteAIDelegate del, Form1.ToggleAIButtonsDelegate del2)
        {
            this.AIOut = del;
            this.ToggleDel = del2;

            r = new Random();
            form1 = form;
            aiTimer = new Timer(100);
            aiTimer.Elapsed += new ElapsedEventHandler(aiTimer_Tick);
            aiTimer.Stop();
            aiIsRunning = false;
        }

        #region Control
        public void Start()
        {
            gameState = form1.gameState;

            if (aiIsRunning)
            {
                form1.WriteAI("ERROR: AI is already running");
                return;
            }
            if (!gameState.processRunning)
            {
                form1.WriteAI("ERROR: Halo is not running");
                return;
            }
            if (root == null)
            {
                form1.WriteAI("ERROR: Please load a valid AI script");
                return;
            }

            form1.nav.AimAhead = false;
            form1.nav.aimbot.LookAheadUnlock();
            form1.nav.aimbot.NavUnlock();
            form1.nav.StrafeMode = false;
            form1.nav.aimbot.arcMode = false;
            form1.nav.aimbot.GravityScale = 1;
            form1.nav.aimbot.ProjectileVelocity = 0;
            dataSources[(int)DATA_SOURCES.VALUE1] = 0;
            dataSources[(int)DATA_SOURCES.VALUE2] = 0;
            dataSources[(int)DATA_SOURCES.VALUE3] = 0;
            dataSources[(int)DATA_SOURCES.VALUE4] = 0;
            dataSources[(int)DATA_SOURCES.VALUE5] = 0;

            form1.startAIButton.Enabled = false;
            form1.stopAIButton.Enabled = true;

            ready = true;
            aiTimer.Start();
            aiIsRunning = true;
        }

        public void Stop()
        {
            ready = false;
            aiTimer.Stop();
            form1.nav.Stop();
            form1.nav.aimbot.Pause();
            form1.nav.StrafeMode = false;
            form1.Invoke(ToggleDel, false);
            form1.gameState.BSPloaded = false;
            aiIsRunning = false;
        }

        public void ChangeInterval(int ticks)
        {
            this.aiTimer.Interval = ticks;
        }
        #endregion

        public void WriteAICrossThread(string arg, bool debug)
        {
            if (DebugPrinting && debug)
                form1.Invoke(AIOut, arg);
            else if (!DebugPrinting && !debug)
                form1.Invoke(AIOut, arg);
        }


        //~~~~~~SEPARATE THREAD, cross thread ops may be needed~~~~~~
        void aiTimer_Tick(object sender, EventArgs e)
        {
            if (!ready)
            {
                WriteAICrossThread(
                    "WARNING: AI cycle taking longer to perform than refresh interval (increase interval)", true);
                return;
            }

			ready = false;
            random = r.Next();
            WriteAICrossThread("Tick: " + random.ToString("X"), true);
            root.Execute();
			ready = true;
        }

        //-------------------------------------------------------------
        // DATA SOURCES GIVEN VALUES HERE
        //-------------------------------------------------------------
        public double RequestData(int dataSource)
        {
            if (sourceTouches[dataSource] != random)
            {
                switch (dataSource)
                {
                    //sources: value1-5 dont need cases, works anyway

                    //LOCAL COORDINATES AND ANGLES
                    case (int)DATA_SOURCES.PLAYER_X:
                        Structures.FLOAT3 pos = gameState.LocalPosition;
                        dataSources[(int)DATA_SOURCES.PLAYER_X] = pos.X;
                        dataSources[(int)DATA_SOURCES.PLAYER_Y] = pos.Y;
                        dataSources[(int)DATA_SOURCES.PLAYER_Z] = pos.Z;
                        sourceTouches[(int)DATA_SOURCES.PLAYER_Y] = random;
                        sourceTouches[(int)DATA_SOURCES.PLAYER_Z] = random;
                        break;
                    case (int)DATA_SOURCES.PLAYER_Y:
                        pos = gameState.LocalPosition;
                        dataSources[(int)DATA_SOURCES.PLAYER_X] = pos.X;
                        dataSources[(int)DATA_SOURCES.PLAYER_Y] = pos.Y;
                        dataSources[(int)DATA_SOURCES.PLAYER_Z] = pos.Z;
                        sourceTouches[(int)DATA_SOURCES.PLAYER_X] = random;
                        sourceTouches[(int)DATA_SOURCES.PLAYER_Z] = random;
                        break;
                    case (int)DATA_SOURCES.PLAYER_Z:
                        pos = gameState.LocalPosition;
                        dataSources[(int)DATA_SOURCES.PLAYER_X] = pos.X;
                        dataSources[(int)DATA_SOURCES.PLAYER_Y] = pos.Y;
                        dataSources[(int)DATA_SOURCES.PLAYER_Z] = pos.Z;
                        sourceTouches[(int)DATA_SOURCES.PLAYER_X] = random;
                        sourceTouches[(int)DATA_SOURCES.PLAYER_Y] = random;
                        break;

                    case (int)DATA_SOURCES.VIEW_ANGLE_H:
                        dataSources[(int)DATA_SOURCES.VIEW_ANGLE_H] = (double)gameState.PlayerHorizontalViewAngle;
                        break;
                    case (int)DATA_SOURCES.VIEW_ANGLE_V:
                        dataSources[(int)DATA_SOURCES.VIEW_ANGLE_V] = (double)gameState.PlayerVerticalViewAngle;
                        break;

                    case (int)DATA_SOURCES.CAMERA_X:
                        pos = gameState.CameraPosition;
                        dataSources[(int)DATA_SOURCES.CAMERA_X] = pos.X;
                        dataSources[(int)DATA_SOURCES.CAMERA_Y] = pos.Y;
                        dataSources[(int)DATA_SOURCES.CAMERA_Z] = pos.Z;
                        sourceTouches[(int)DATA_SOURCES.CAMERA_Y] = random;
                        sourceTouches[(int)DATA_SOURCES.CAMERA_Z] = random;
                        break;
                    case (int)DATA_SOURCES.CAMERA_Y:
                        pos = gameState.CameraPosition;
                        dataSources[(int)DATA_SOURCES.CAMERA_X] = pos.X;
                        dataSources[(int)DATA_SOURCES.CAMERA_Y] = pos.Y;
                        dataSources[(int)DATA_SOURCES.CAMERA_Z] = pos.Z;
                        sourceTouches[(int)DATA_SOURCES.CAMERA_X] = random;
                        sourceTouches[(int)DATA_SOURCES.CAMERA_Z] = random;
                        break;
                    case (int)DATA_SOURCES.CAMERA_Z:
                        pos = gameState.CameraPosition;
                        dataSources[(int)DATA_SOURCES.CAMERA_X] = pos.X;
                        dataSources[(int)DATA_SOURCES.CAMERA_Y] = pos.Y;
                        dataSources[(int)DATA_SOURCES.CAMERA_Z] = pos.Z;
                        sourceTouches[(int)DATA_SOURCES.CAMERA_X] = random;
                        sourceTouches[(int)DATA_SOURCES.CAMERA_Y] = random;
                        break;


                    //MISC
                    case (int)DATA_SOURCES.CAN_WALK:
                        bool cw = gameState.CanWalk;
                        dataSources[(int)DATA_SOURCES.CAN_WALK] = cw ? 1 : 0;
                        break;
                    case (int)DATA_SOURCES.CLEAR_SHOT:
                        dataSources[dataSource] = gameState.HasClearShot ? 1.0 : 0.0;
                        break;
                    case (int)DATA_SOURCES.CLEAR_SHOT_ADVANCED:
                        dataSources[dataSource] = gameState.BSPIntersectionCheck(
                            gameState.LocalPosition, gameState.PlayerPosition(form1.nav.aimbot.GetTargetIndex()))
                            ? 0.0 : 1.0;
                        break;
                    case (int)DATA_SOURCES.SHIFT_KEY:
                        dataSources[dataSource] = Form1.GetKeyState(Form1.VirtualKeyStates.VK_SHIFT) < 0 ?
                            1.0 : 0.0;
                        break;
                    case (int)DATA_SOURCES.PLAYER_COUNT:
                        dataSources[dataSource] = (double)gameState.PlayerCount;
                        break;
                    case (int)DATA_SOURCES.RANDOM:
                        dataSources[dataSource] = r.NextDouble();
                        break;

                    //LOCAL INFO
                    case (int)DATA_SOURCES.LOCAL_HEALTH:
                        dataSources[dataSource] = (double)gameState.LocalHealth;
                        break;
                    case (int)DATA_SOURCES.TARGET_HEALTH:
                        dataSources[dataSource] =
                            (double)gameState.PlayerHealth(form1.nav.aimbot.GetTargetIndex());
                        break;
                    case (int)DATA_SOURCES.TARGET_DIST:
                        dataSources[dataSource] =
                            (double)Navigation.Distance3D(form1.nav.aimbot.GetTargetPos(), gameState.LocalPosition);
                        break;
                    case (int)DATA_SOURCES.TARGET_SHIELD:
                        dataSources[dataSource] =
                            (double)gameState.PlayerShield(form1.nav.aimbot.GetTargetIndex());
                        break;
                    case (int)DATA_SOURCES.LOCAL_SHIELD:
                        dataSources[dataSource] = (double)gameState.LocalShield;
                        break;
                    case (int)DATA_SOURCES.LOCAL_TEAM:
                        dataSources[dataSource] = (double)gameState.LocalTeam;
                        break;
                    case (int)DATA_SOURCES.LOCAL_PING:
                        dataSources[dataSource] = (double)gameState.LocalPing;
                        break;
                    case (int)DATA_SOURCES.ZOOM_LEVEL:
                        dataSources[dataSource] = (double)gameState.ZoomLevel;
                        break;
                    case (int)DATA_SOURCES.FLASHLIGHT:
                        dataSources[dataSource] = gameState.FlashlightOn ? 1.0 : 0.0;
                        break;

                    //WEAPONS
                    case (int)DATA_SOURCES.PRIMARY_WEAPON:
                        dataSources[dataSource] = gameState.PrimaryWeapon ? 1.0 : 0.0;
                        break;
                    case (int)DATA_SOURCES.WEAPON0_TYPE:
                        dataSources[dataSource] = gameState.GetWeaponType(
                            gameState.GetObjectOffset(
                            gameState.GetWeaponIndex(gameState.LocalIndex, true)));
                        break;
                    case (int)DATA_SOURCES.WEAPON1_TYPE:
                        dataSources[dataSource] = gameState.GetWeaponType(
                            gameState.GetObjectOffset(
                            gameState.GetWeaponIndex(gameState.LocalIndex, false)));
                        break;
                    case (int)DATA_SOURCES.WEAPON0_CLIP:
                        dataSources[dataSource] = gameState.GetWeaponClip(
                            gameState.GetObjectOffset(
                            gameState.GetWeaponIndex(gameState.LocalIndex, true)));
                        break;
                    case (int)DATA_SOURCES.WEAPON1_CLIP:
                        dataSources[dataSource] = gameState.GetWeaponClip(
                            gameState.GetObjectOffset(
                            gameState.GetWeaponIndex(gameState.LocalIndex, false)));
                        break;
                    case (int)DATA_SOURCES.WEAPON0_RESERVE:
                        dataSources[dataSource] = gameState.GetWeaponReserve(
                            gameState.GetObjectOffset(
                            gameState.GetWeaponIndex(gameState.LocalIndex, true)));
                        break;
                    case (int)DATA_SOURCES.WEAPON1_RESERVE:
                        dataSources[dataSource] = gameState.GetWeaponReserve(
                            gameState.GetObjectOffset(
                            gameState.GetWeaponIndex(gameState.LocalIndex, false)));
                        break;
                    case (int)DATA_SOURCES.GRENADE_TYPE:
                        dataSources[dataSource] = (double)gameState.LocalGrenadeType;
                        break;
                    case (int)DATA_SOURCES.PLASMA_GRENADE_COUNT:
                        dataSources[dataSource] = (double)gameState.LocalPlasmaGrenadeCount;
                        break;
                    case (int)DATA_SOURCES.FRAG_GRENADE_COUNT:
                        dataSources[dataSource] = (double)gameState.LocalFragGrenadeCount;
                        break;

                    //TARGET INFO
                    case(int)DATA_SOURCES.TARGET_CAMO:
                        dataSources[dataSource] = gameState.PlayerHasCamo(form1.nav.aimbot.GetTargetIndex()) ? 1.0 : 0.0;
                        break;
                    case (int)DATA_SOURCES.LOCAL_CAMO:
                        dataSources[dataSource] = gameState.LocalCamo ? 1.0 : 0.0;
                        break;
                    case (int)DATA_SOURCES.TARGET_STANCE:
                        dataSources[dataSource] = (double)gameState.PlayerStance(form1.nav.aimbot.GetTargetIndex());
                        break;
                    case (int)DATA_SOURCES.LOCAL_STANCE:
                        dataSources[dataSource] = (double)gameState.LocalStance;
                        break;

                    //GETTING TARGETS
                    case (int)DATA_SOURCES.CLOSEST_ENEMY:
                    case (int)DATA_SOURCES.CLOSEST_ALLY:
                    case (int)DATA_SOURCES.CLOSEST_ANYONE:
                    //call FID CLOSEST_ENEMY/ALLY/ANYONE before CLOSEST_DIST to set DIST's mode
                    case (int)DATA_SOURCES.CLOSEST_DIST:
                        #region CLOSEST_PLAYER
                        pos = gameState.LocalPosition;
                        int localTeam = gameState.LocalTeam;

                        int maxSlots = gameState.MaxSlots;
                        int closestIndex = -1;
                        float closestDist = float.MaxValue;

                        //for every player
                        for (int i = 0; i < maxSlots; i++)
                        {
                            int staticPlayerPointer = gameState.GetStaticPlayerPointer(i);
                            ushort objectID = gameState.GetPlayerObjectID(staticPlayerPointer);
                            ushort objectIndex = gameState.GetPlayerObjectIndex(staticPlayerPointer);
                            short playerId2 = gameState.GetPlayerId2(staticPlayerPointer);

                            if (objectID == 65535 || objectID == 0 || objectIndex == gameState.LocalObjectIndex ||
                                objectID != gameState.GetObjectID(objectIndex))// || playerId2 == -1)
                                continue;

                            if (dataSource == (int)DATA_SOURCES.CLOSEST_ENEMY ||
                                dataSource == (int)DATA_SOURCES.CLOSEST_DIST)
                            {
                                if (gameState.GetPlayerTeam(staticPlayerPointer) == localTeam)
                                    continue;
                            }
                            else if (dataSource == (int)DATA_SOURCES.CLOSEST_ALLY)
                            {
                                if (gameState.GetPlayerTeam(staticPlayerPointer) != localTeam)
                                    continue;
                            }

                            float dist = Navigation.Distance3D(gameState.PlayerPosition(i), pos);
                            if (dist < closestDist)
                            {
                                closestDist = dist;
                                closestIndex = i;
                            }
                        }

                        dataSources[dataSource] = (double)closestIndex;
                        dataSources[(int)DATA_SOURCES.CLOSEST_DIST] = (double)closestDist;

                        if (dataSource == (int)DATA_SOURCES.CLOSEST_DIST)
                            sourceTouches[(int)DATA_SOURCES.CLOSEST_ENEMY] = random;
                        else
                            sourceTouches[(int)DATA_SOURCES.CLOSEST_DIST] = random;

                        break;
                        #endregion

                    case (int)DATA_SOURCES.ENEMY_NEAREST_VIEW:
                    case (int)DATA_SOURCES.ALLY_NEAREST_VIEW:
                    case (int)DATA_SOURCES.ANYONE_NEAREST_VIEW:
                        #region PLAYER_NEAREST_VIEW
                        pos = gameState.LocalPosition;
                        localTeam = gameState.LocalTeam;

                        maxSlots = gameState.MaxSlots;
                        closestIndex = -1;
                        float closestAngle = float.MaxValue;

                        //for every player
                        for (int i = 0; i < maxSlots; i++)
                        {
                            int staticPlayerPointer = gameState.GetStaticPlayerPointer(i);
                            ushort objectID = gameState.GetPlayerObjectID(staticPlayerPointer);
                            ushort objectIndex = gameState.GetPlayerObjectIndex(staticPlayerPointer);

                            if (objectID == 65535 || objectID == 0 || objectIndex == gameState.LocalObjectIndex ||
                                objectID != gameState.GetObjectID(objectIndex))
                                continue;

                            if (dataSource == (int)DATA_SOURCES.ENEMY_NEAREST_VIEW)
                            {
                                if (gameState.GetPlayerTeam(staticPlayerPointer) == localTeam)
                                    continue;
                            }
                            else if (dataSource == (int)DATA_SOURCES.ALLY_NEAREST_VIEW)
                            {
                                if (gameState.GetPlayerTeam(staticPlayerPointer) != localTeam)
                                    continue;
                            }

                            //calculate angle from view center to player i
                            float closeness = form1.nav.aimbot.TargetClosenessToView(gameState.PlayerPosition(i));

                            //compare angle to previously closest
                            if (closeness < closestAngle)
                            {
                                closestAngle = closeness;
                                closestIndex = i;
                            }
                        } //for every player

                        dataSources[dataSource] = (double)closestIndex;
                        break;
                        #endregion
                    case (int)DATA_SOURCES.RANDOM_NODE:
                        //if no nodes, do nothing
                        if (form1.graph.pool[0] == null)
                            break;

                        //otherwise, find a node that exists (may have holes in the array)
                        int node;
                        do
                            node = r.Next(1, form1.graph.LastIndex + 1);
                        while (form1.graph.pool[node] == null);
                        dataSources[dataSource] = node;
                        break;
                } //end switch

                sourceTouches[dataSource] = random;
            } //if requested data source hasn't been calculated this tick

            return dataSources[dataSource];
        }

        public void setDataSource(int index, double value)
        {
            dataSources[index] = value;
        }
        
        //loads the AI text file
        public bool Load(Stream myStream)
        {
            Stop();
            form1.WriteAI("Reading file...");
            StreamReader sr = new StreamReader(myStream);

            try
            {
                root = new Subtask("root", this, "1");
            }
            catch (InvalidOperationException)
            {
                return false;
            }

            if (ParseLevel(root, sr))
                form1.WriteAI("Successfully loaded task tree");
            else
            {
                form1.WriteAI("ERROR: Could not load task tree");
                root = null;
                return false;
            }
            return true;
        }

        //parses a Subtask's block of children and adds the children under Subtask
        private bool ParseLevel(Subtask parent, StreamReader sr)
        {
            if (DebugPrinting)
                form1.WriteAI("DEBUG: Searching for child tasks in \"" + parent.ToString() + "\"");

            char[] buffer = new char[1];
            String temp;
            bool concurrent = false;

            //pass through every char until we reach something of interest (namely a child or >)
            while (sr.Peek() != -1 && (char)sr.Peek() != '}')
            {
                sr.Read(buffer, 0, 1);
                temp = new String(buffer);

                //decide what to do about the char read in
                switch (buffer[0])
                {
                    case '#':
                        String comment = sr.ReadLine();
                        if (DebugPrinting)
                            form1.WriteAI("DEBUG: Skipping comment \"#" + comment + "\"");
                        if (comment.StartsWith("!"))
                        {
                            int t;
                            if (Int32.TryParse(comment.Substring(1), out t))
                                form1.setAIInterval(t);
                            form1.WriteAI("DEBUG: Setting AI Interval to " + t);
                        }
                        break;
                    case '[':
                        //makenode will move sr ahead to the next header in this block
                        if (DebugPrinting)
                            form1.WriteAI("DEBUG: Encountered a child task in \"" + parent.ToString() + "\"");
                        TaskNode child = MakeNode(sr, concurrent);
                        if (child == null)
                        {
                            form1.WriteAI("ERROR: Failed to create child task in \"" + parent.ToString() + "\"");
                            return false;
                        }
                        parent.children.Add(child);
                        concurrent = false;
                        break;
                    case '*':
                        concurrent = true;
                        break;
                    case '>':
                        String path = form1.launchPath + "\\ai\\" + sr.ReadLine();
                        if (DebugPrinting)
                            form1.WriteAI("DEBUG: Including external file \"" + path + "\"");
                        FileStream fs;
                        try
                        {
                            fs = File.Open(path, FileMode.Open);
                        }
                        catch (IOException)
                        {
                            form1.WriteAI("ERROR: Could not find included file \"" + path + "\"");
                            return false;
                        }
                        StreamReader tempStream = new StreamReader(fs);
                        if (!ParseLevel(parent, tempStream))
                            return false;
                        break;
                    default:
                        if (DebugPrinting)
                        {
                            form1.WriteAI("DEBUG: Skipping character \"" + temp + "\"");
                            form1.WriteAI("(" + ((int)(temp.ToCharArray()[0])).ToString() + ")");
                        }
                        break;
                }
            } //for each char between things of interest
            
            //at the block ending char '}'

            return true;
        }

        //checks if a char is in a char array
        private bool CharInList(char check, char[] delims)
        {
            foreach (char c in delims)
            {
                if (c == check)
                    return true;
            }
            return false;
        }

        //reads chars from the stream into a buffer until it finds a delim char
        private bool ReadUpTo(StreamReader sr, char[] buffer, char[] delims)
        {
            int i = 0;
            while (!CharInList((char)sr.Peek(), delims))
            {
                if (i == buffer.Length - 1)
                    return false;

                sr.Read(buffer, i, 1);
                i++;
            }

            for (; i < buffer.Length; i++)
                buffer[i] = ' ';
            return true;
        }

        private TaskNode MakeNode(StreamReader sr, bool concurrent)
        {
            TaskNode p;

            if (DebugPrinting)
                form1.WriteAI("DEBUG: New TaskNode is " + ((concurrent) ? "concurrent" : "not concurrent" ));

            //read in the information for the encountered node (storing in temp vars)
            //currently sr is right after the "["

            #region read_postfix
            //scan up to a ] meaning we copied in the postfix expression
            char[] buffer = new char[MAX_STRING_BUFFER_SIZE];
            char[] delims = {']'};
            if (!ReadUpTo(sr, buffer, delims))
            {
                form1.WriteAI("ERROR: Overran buffer in search for ']'");
                return null;
            }
            String postfix = new String(buffer);
            postfix = postfix.Trim();
            if (DebugPrinting)
                form1.WriteAI("DEBUG: Created priority expression string \"" + postfix + "\"");
            #endregion

            //move over the ]
            sr.Read();

            #region read_name
            //scan up to a ! or a { (so that we'll skip over and record a name)
            char[] delims2 = { '!', '{' };
            if (!ReadUpTo(sr, buffer, delims2))
            {
                form1.WriteAI("ERROR: Overran buffer in search for '!' or '{'");
                return null;
            }
            String name = new String(buffer);
            name = name.Trim();
            if (DebugPrinting)
                form1.WriteAI("DEBUG: Created name \"" + name + "\"");
            #endregion

            char next = (char)sr.Read();

            //if encountered a block start "{"
            if (next == '{')
            {
                #region make_subtask
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Found a '{', calling ParseLevel for new parent \"" + name + "\"");

                //make p a new subtask (handle exception)
                try
                {
                    p = new Subtask(name, this, postfix);
                }
                catch (InvalidOperationException)
                {
                    form1.WriteAI("ERROR: Failed to construct FunctionNode \"" + name + "\" with given postfix expression");
                    return null;
                }

                //call ParseLevel with parent node p
                if (!ParseLevel((Subtask)p, sr))
                    return null;

                //make sure "}" termination
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Serching for '}' termination for \"" + name + "\"");

                while ((char)sr.Read() != '}') ;

                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Found '}' termination for Subtask, returning");
                #endregion
            }
            else if (next == '!')
            {
                #region make_function_node
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Found a '!', creating FunctionNode \"" + name + "\"");

                // *[<postfix priority expression>] !<action>(<param>);

                #region read_FID
                char[] delims3 = { '(', ')' };
                if (!ReadUpTo(sr, buffer, delims3))
                {
                    form1.WriteAI("ERROR: Overran buffer in search for '(' for \"" + name + "\"");
                    return null;
                }
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Read up to a '(' to get FID");

                String temp = new String(buffer);
                int functionId;
                if (!Int32.TryParse(temp, out functionId))
                {
                    form1.WriteAI("ERROR: Invalid FID \"" + temp + "\" in \"" + name + "\"");
                    return null;
                }
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Parsed FID into int \"" + functionId.ToString() + "\"");
                #endregion

                //skip over '('
                sr.Read();

                #region read_param
                //get the param
                if (!ReadUpTo(sr, buffer, delims3))
                {
                    form1.WriteAI("ERROR: Overran buffer in search for ')' for \"" + name + "\"");
                    return null;
                }
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Read up to a ')' to get param");

                String param = new String(buffer);
                param = param.Trim();
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Created parameter expression string \"" + param + "\"");
                #endregion

                //make p a new function node (handle exception)
                try
                {
                    p = new FunctionNode(this, name, functionId, param, this, postfix);
                }
                catch (InvalidOperationException)
                {
                    form1.WriteAI("ERROR: Failed to construct FunctionNode \"" + name + "\" with given postfix expression");
                    return null;
                }
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: FunctionNode \"" + name + "\" created");

                //skip ')'
                sr.Read();
                //make sure ";" termination
                if ((char)sr.Read() != ';')
                {
                    form1.WriteAI("ERROR: Expected a ';' closing for \"" + name + "\"");
                    return null;
                }
                if (DebugPrinting)
                    form1.WriteAI("DEBUG: Found ';' termination for FunctionNode, returning");
                #endregion
            }
            else
            {
                form1.WriteAI("ERROR: Expected a block start '{' or FID '!' for \"" + name + "\"");
                return null;
            }

            p.Concurrent = concurrent;

            return p;
        }

    }
}
