using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace HaloBot
{
    public abstract class TaskNode
    {
        protected String name = "";
        protected ExpressionNode priorityRoot;
        public bool Concurrent;

        protected static int ComparePriorities(TaskNode a, TaskNode b)
        {
            //return what a is compared to b (-1 is less than, 0 equal, 1 greater)
            double priorityA = a.CalcPriority();
            double priorityB = b.CalcPriority();

            if (priorityA > priorityB)
                return 1;
            if (priorityA < priorityB)
                return -1;
            return 0;
        }

        //builds the TaskNode's priority evalutation tree using a postfix mathematical expression
        protected bool BuildPriorityTree(AIHandler overlord, String priorityExpression, ref ExpressionNode givenRoot)
        {
            char[] splitLegend = {' '};
            String[] expression = priorityExpression.Split(splitLegend, System.StringSplitOptions.RemoveEmptyEntries);

            //create the tree using a stack method, then assign the end result to the priorityRoot field
            Stack<ExpressionNode> stack = new Stack<ExpressionNode>();
            foreach (String s in expression)
            {
                if (s.Equals("+") || s.Equals("-") || s.Equals("/") || s.Equals("*") ||
                    s.Equals("=") || s.Equals("<") || s.Equals(">") || s.Equals("^") ||
                    s.Equals("|") || s.Equals("&") || s.Equals("%") || s.Equals("`") ||
                    s.Equals("~"))
                {
                    //create an operator, pop two from stack and set them as children, push operator
                    ExpressionNode left = stack.Pop();
                    ExpressionNode right = stack.Pop();
                    Operator op = new Operator(s.ToCharArray()[0], left, right);
                    
                    stack.Push(op);
                }
                else
                {
                    //create an operand, push to stack

                    //check if the operand is a variable or a constant
                    if (s.StartsWith("$"))
                    {
                        int source;
                        if (!Int32.TryParse(s.Substring(1), out source))
                        {
                            overlord.form1.WriteAI("ERROR: Invalid variable \"" + s + "\" in \"" + ToString() + "\"");
                            return false;
                        }
                        Operand c = new Operand(overlord, 0, false, source);
                        stack.Push(c);
                    }
                    else
                    {
                        //parse the string into a double d
                        double d;
                        if (!Double.TryParse(s, out d))
                        {
                            overlord.form1.WriteAI("ERROR: Invalid constant \"" + s + "\" in \"" + ToString() + "\"");
                            return false;
                        }
                        Operand c = new Operand(overlord, d, true, 0);
                        stack.Push(c);
                    }
                }
            } //foreach term in the expression

            if (stack.Count != 1)
            {
                overlord.form1.WriteAI("ERROR: Invalid postfix expression \"" + priorityExpression + "\"");
                return false;
            }

            givenRoot = stack.Pop();

            return true;
        }

        public double CalcPriority()
        {
            return priorityRoot.Evaluate();
        }

        public abstract void Execute();
    }

    public class Subtask : TaskNode
    {
        public List<TaskNode> children;

        public Subtask(String name, AIHandler overlord, String priorityExpression)
        {
            this.name = name;
            children = new List<TaskNode>();
            if (!BuildPriorityTree(overlord, priorityExpression, ref priorityRoot))
                throw new InvalidOperationException();
        }

        public override void Execute()
        {
            //sort children by priority
            children.Sort(ComparePriorities);

            bool onConcurrent = true;
            //assume the highest priority event is concurrent, now execute each child in descending priority order
            //until we've executed a non-concurrent one
            for (int i = children.Count - 1; onConcurrent == true && i >= 0; i--)
            {
                TaskNode current = children[i];
                onConcurrent = current.Concurrent;
                current.Execute();
            }
        }

        public override string ToString()
        {
            return name;
        }
    }

    public class FunctionNode : TaskNode
    {
        private int functionId;
        private ExpressionNode paramRoot = null;
        AIHandler aiHandler;

        public FunctionNode(AIHandler aiHandler, String name, int functionId,
            String param, AIHandler overlord, String priorityExpression)
        {
            this.aiHandler = aiHandler;
            this.name = name;
            if (!BuildPriorityTree(overlord, priorityExpression, ref priorityRoot))
                throw new InvalidOperationException();
            if (!BuildPriorityTree(overlord, param, ref paramRoot))
                throw new InvalidOperationException();

            this.functionId = functionId;
        }

        public override void Execute()
        {
            //evaluate the paramter expression of this FunctionNode
            double parameter = paramRoot.Evaluate();

            aiHandler.WriteAICrossThread("Executing FID " + functionId +
                " with parameter " + parameter, true);

            //call the function by FID, pass param
            switch (functionId)
            {
                //MISC
                case (int)AIHandler.FID.SLEEP:
                    System.Threading.Thread.Sleep((int)parameter);
                    break;
                case (int)AIHandler.FID.PRINT:
                    aiHandler.WriteAICrossThread(
                        aiHandler.random.ToString("X") + "\t(" + name + "): " + parameter, false);
                    break;

                //PATHFINDING
                case (int)AIHandler.FID.GOTO_NODE:
                    aiHandler.form1.nav.WalkTo((ushort)parameter, false);
                    break;
                case (int)AIHandler.FID.GOTO_NODE_ALT:
                    aiHandler.form1.nav.WalkTo((ushort)parameter, true);
                    break;
                case (int)AIHandler.FID.GOTO_PLAYER:
                    aiHandler.form1.nav.WalkTo(aiHandler.form1.gameState.PlayerPosition((int)parameter),
                        false);
                    break;
                case (int)AIHandler.FID.GOTO_PLAYER_ALT:
                    aiHandler.form1.nav.WalkTo(aiHandler.form1.gameState.PlayerPosition((int)parameter),
                        true);
                    break;
                case (int)AIHandler.FID.GOTO_OBJECTIVE:
                    aiHandler.form1.nav.WalkTo(aiHandler.form1.gameState.ObjectivePosition((int)parameter),
                        false);
                    break;
                case (int)AIHandler.FID.GOTO_OBJECTIVE_ALT:
                    aiHandler.form1.nav.WalkTo(aiHandler.form1.gameState.ObjectivePosition((int)parameter),
                        true);
                    break;

                //INPUT
                case (int)AIHandler.FID.MOUSE1:
                    aiHandler.form1.nav.Click(true, (int)parameter);
                    break;
                case (int)AIHandler.FID.MOUSE2:
                    aiHandler.form1.nav.Click(false, (int)parameter);
                    break;
                case (int)AIHandler.FID.CROUCH:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_LCONTROL, (int)parameter, true);
                    break;
                case (int)AIHandler.FID.JUMP:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_SPACE, (int)parameter, true);
                    break;
                case (int)AIHandler.FID.SWITCH_WEAPONS:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_TAB, 35, true);
                    break;
                case (int)AIHandler.FID.MELEE:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_F, 35, true);
                    break;
                case (int)AIHandler.FID.RELOAD:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_R, 35, true);
                    break;
                case (int)AIHandler.FID.ZOOM:
                    if (parameter != 0 && parameter != 1)
                        return;
                    int prevLevel = -1;
                    int currentLevel;
                    while ((currentLevel = aiHandler.gameState.ZoomLevel) != parameter)
                    {
                        if (currentLevel == prevLevel)
                            break;
                        aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_Z, 35, true);
                        prevLevel = aiHandler.gameState.ZoomLevel;
                    }
                    break;
                case (int)AIHandler.FID.FLASHLIGHT:
                    if (parameter == 0 && aiHandler.gameState.FlashlightOn)
                        aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_Q, 35, true);
                    if (parameter != 0 && !aiHandler.gameState.FlashlightOn)
                        aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_Q, 35, true);
                    break;
                case (int)AIHandler.FID.BACKPACK_RELOAD:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_R, 35, true);
                    System.Threading.Thread.Sleep(50);
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_R, 35, true);
                    System.Threading.Thread.Sleep(50);
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_TAB, 35, true);
                    break;
                case (int)AIHandler.FID.EXCHANGE_WEAPON:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_X, 35, true);
                    break;
                case (int)AIHandler.FID.ACTION:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_E, 35, true);
                    break;
                case (int)AIHandler.FID.SWITCH_GRENADE_TYPE:
                    aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_G, 35, true);
                    break;

                //STORAGE VALUES
                case (int)AIHandler.FID.SET_VALUE1:
                    aiHandler.setDataSource((int)AIHandler.DATA_SOURCES.VALUE1, parameter);
                    break;
                case (int)AIHandler.FID.SET_VALUE2:
                    aiHandler.setDataSource((int)AIHandler.DATA_SOURCES.VALUE2, parameter);
                    break;
                case (int)AIHandler.FID.SET_VALUE3:
                    aiHandler.setDataSource((int)AIHandler.DATA_SOURCES.VALUE3, parameter);
                    break;
                case (int)AIHandler.FID.SET_VALUE4:
                    aiHandler.setDataSource((int)AIHandler.DATA_SOURCES.VALUE4, parameter);
                    break;
                case (int)AIHandler.FID.SET_VALUE5:
                    aiHandler.setDataSource((int)AIHandler.DATA_SOURCES.VALUE5, parameter);
                    break;

                //AIMBOT
                case (int)AIHandler.FID.TOGGLE_AIMBOT:
                    if (parameter != 0)
                        aiHandler.form1.nav.aimbot.Start();
                    else
                        aiHandler.form1.nav.aimbot.Pause();
                    break;
                case (int)AIHandler.FID.SET_TARGET_PLAYER:
                    aiHandler.form1.nav.aimbot.SetTarget((int)parameter, true);
                    break;
                case (int)AIHandler.FID.SET_TARGET_NODE:
                    if (aiHandler.form1.graph.pool[(int)parameter] == null)
                        break;
                    aiHandler.form1.nav.aimbot.SetTarget(aiHandler.form1.graph.pool[(int)parameter].pos, true);
                    break;
                case (int)AIHandler.FID.AIMBOT_ENABLE_ARC_MODE:
                    aiHandler.form1.nav.aimbot.arcMode = true;
                    aiHandler.form1.nav.aimbot.GravityScale = (float)parameter;
                    break;
                case (int)AIHandler.FID.AIMBOT_DISABLE_ARC_MODE:
                    aiHandler.form1.nav.aimbot.arcMode = false;
                    break;
                case (int)AIHandler.FID.SET_PROJECTILE_VELOCITY:
                    aiHandler.form1.nav.aimbot.ProjectileVelocity = (float)parameter;
                    break;

                //PATH FOLLOWING
                case (int)AIHandler.FID.SET_STRAFE_MODE:
                    aiHandler.form1.nav.StrafeMode = (parameter == 0) ? false : true;
                    break;
                case (int)AIHandler.FID.SET_LOOK_AHEAD_MODE:
                    aiHandler.form1.nav.AimAhead = (parameter == 0) ? false : true;
                    break;
                
                //WEAPONS
                case (int)AIHandler.FID.SET_PRIMARY_WEAPON:
                    ushort weapon1Index = aiHandler.gameState.GetWeaponIndex(
                        aiHandler.gameState.LocalIndex, false);
                    ushort primaryIndex = aiHandler.gameState.GetPrimaryWeaponIndex(
                        aiHandler.gameState.LocalIndex);
                    bool current = weapon1Index == primaryIndex;

                    if ((parameter != 0) != current)
                    {
                        aiHandler.form1.nav.Press((ushort)Navigation.DIK.DIK_TAB, 35, true);
                    }
                    break;

                case (int)AIHandler.FID.CHAT:
                    String filename = aiHandler.form1.launchPath + "\\chat\\" + (int)parameter + ".txt";
                    if (!aiHandler.form1.nav.Chat(filename))
                        aiHandler.WriteAICrossThread("Failed to type chat message " +
                            parameter, true);
                    break;

                default:
                    break;
            }
        }

        public override string ToString()
        {
            return name + " !" + functionId.ToString();
        }
    }

    #region Expression_Tree_Stuff

    public abstract class ExpressionNode
    {
        public abstract double Evaluate();
    }

    public class Operator : ExpressionNode
    {
        private char op;
        private ExpressionNode left;
        private ExpressionNode right;

        public Operator(char op, ExpressionNode left, ExpressionNode right)
        {
            this.op = op;
            this.left = left;
            this.right = right;
        }

        public override double Evaluate()
        {
            switch (op)
            {
                case '+':
                    return right.Evaluate() + left.Evaluate();
                case '-':
                    return right.Evaluate() - left.Evaluate();
                case '/':
                    return right.Evaluate() / left.Evaluate();
                case '*':
                    return right.Evaluate() * left.Evaluate();
                case '=':
                    return right.Evaluate() == left.Evaluate() ? 1.0 : 0.0;
                case '>':
                    return right.Evaluate() > left.Evaluate() ? 1.0 : 0.0;
                case '<':
                    return right.Evaluate() < left.Evaluate() ? 1.0 : 0.0;
                case '|':
                    return (right.Evaluate() != 0.0 || left.Evaluate() != 0.0) ? 1.0 : 0.0;
                case '&':
                    return (right.Evaluate() != 0.0 && left.Evaluate() != 0.0) ? 1.0 : 0.0;
                case '^':
                    return Math.Pow(right.Evaluate(), left.Evaluate());
                case '%':
                    return right.Evaluate() % left.Evaluate();
                case '`':
                    return Math.Max(right.Evaluate(), left.Evaluate());
                case '~':
                    return Math.Min(right.Evaluate(), left.Evaluate());

            }

            return 0.0;
        }
    }

    public class Operand : ExpressionNode
    {
        private double value;
        private bool constant;
        private int dataSource;
        private AIHandler overlord;

        public Operand(AIHandler overlord, double value, bool constant, int dataSource)
        {
            this.overlord = overlord;
            this.value = value;
            this.constant = constant;
            this.dataSource = dataSource;
        }

        public override double Evaluate()
        {
            if (constant)
                return value;
            return overlord.RequestData(dataSource);
        }
    }

#endregion

}
