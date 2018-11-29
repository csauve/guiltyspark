using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics;

namespace HaloBot
{
	[Serializable()]
	public class Graph : ISerializable
	{
		public Waypoint[] pool;
		public ushort LastIndex;

		public Graph(ushort initialSize)
		{
			LastIndex = 0;
			pool = new Waypoint[initialSize];
			pool[0] = new Waypoint();
		}

		public Graph(SerializationInfo info, StreamingContext ctxt)
		{
			this.pool = (Waypoint[])info.GetValue("pool", typeof(object));
			this.LastIndex = (ushort)info.GetValue("LastIndex", typeof(ushort));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
			info.AddValue("pool", pool);
			info.AddValue("LastIndex", LastIndex);
		}

		public void ResetCosts()
		{
			for (int i = 0; i <= LastIndex; i++)
				if (pool[i] != null)
				{
					pool[i].Reset();
				}
		}

        public delegate void GraphModifiedHandler(object sender, EventArgs e);
        public event GraphModifiedHandler Modified;

        //equivalent to the function below with second argument 0
        public ushort Add(Structures.FLOAT3 pos)
		{
            for (ushort i = 1; i < pool.Length; i++)
            {
                if (pool[i] == null)
                {
                    Modified(this, new EventArgs());
                    pool[i] = new Waypoint(pos);
                    if (i > LastIndex)
                        LastIndex = i;
                    return i;
                }
            }
            return 0;
		}

        //adds a node and automatically links it to the last placed node
        public ushort Add(Structures.FLOAT3 pos, ushort lastNodeAdded)
        {
            for (ushort i = 1; i < pool.Length; i++)
            {
                if (pool[i] == null)
                {
                    Modified(this, new EventArgs());
                    pool[i] = new Waypoint(pos);
                    if (i > LastIndex)
                        LastIndex = i;

                    if (lastNodeAdded >= 1)
                    {
                        Link(lastNodeAdded, i, 1);
                        Link(i, lastNodeAdded, 1);
                    }
                    return i;
                }
            }

            return 0;
        }

		public bool Remove(ushort index)
		{
            if (!RemoveLinks(index))
                return false;

            Modified(this, new EventArgs());
			pool[index] = null;
            return true;
		}

        public bool RemoveLinks(ushort index)
        {
            if (!CheckParameterValid(index))
                return false;

            Modified(this, new EventArgs());
            for (int i = 1; i <= LastIndex; i++)
                if (pool[i] != null)
                    pool[i].Unlink(index);
            return true;
        }

        public bool Move(ushort index, Structures.FLOAT3 pos)
		{
            if (!CheckParameterValid(index))
                return false;

            Modified(this, new EventArgs());
			pool[index].Move(pos.X, pos.Y, pos.Z);
            return true;
		}

		public bool Link(ushort src, ushort dst, byte type)
		{
            if (!CheckParameterValid(src) || !CheckParameterValid(dst) || src == dst)
                return false;

            Modified(this, new EventArgs());
			return pool[src].Link(dst, type);
		}

        public bool CheckParameterValid(ushort src)
        {
            return !(src < 1 || src > LastIndex || pool[src] == null);
        }
	}

	[Serializable()]
	public class Waypoint : ISerializable
	{
        public Structures.FLOAT3 pos;

		public ushort[] SurroundingIndexes = new ushort[10];
		public byte[] ConnectionTypes = new byte[10];
		public byte NumberOfConnections;

		//used in pathfinding
		[NonSerialized()]
		public ushort Parent;
		[NonSerialized()]
		public float CostG = float.PositiveInfinity;
		[NonSerialized()]
		public float CostH;
        [NonSerialized()]
        public float PersistentCost;

		public Waypoint(Structures.FLOAT3 pos)
		{
            this.pos = pos;
			NumberOfConnections = 0;
		}

		public byte GetLinkType(Waypoint p, Waypoint[] pool)
		{
			for (byte i = 0; i < NumberOfConnections; i++)
			{
				if (pool[SurroundingIndexes[i]].Equals(p))
					return ConnectionTypes[i];
			}
			return 1;
		}

		public bool Equals(Waypoint p)
		{
			return p.pos.X == this.pos.X && p.pos.Y == this.pos.Y && p.pos.Z == this.pos.Z;
		}

		public void Reset()
		{
			Parent = 0;
			CostG = float.PositiveInfinity;
			CostH = 0;
            PersistentCost = (float)Math.Max(PersistentCost - 0.1, 0);
        }

		public float GetFCost()
		{
			return CostG + CostH;
		}

		public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
		{
            info.AddValue("X", pos.X);
            info.AddValue("Y", pos.Y);
            info.AddValue("Z", pos.Z);
			info.AddValue("SurroundingIndexes", SurroundingIndexes);
			info.AddValue("ConnectionTypes", ConnectionTypes);
			info.AddValue("NumberOfConnections", NumberOfConnections);
		}

		public Waypoint(SerializationInfo info, StreamingContext ctxt)
		{
            this.pos.X = (float)info.GetValue("X", typeof(float));
            this.pos.Y = (float)info.GetValue("Y", typeof(float));
            this.pos.Z = (float)info.GetValue("Z", typeof(float));
			this.SurroundingIndexes = (ushort[])info.GetValue("SurroundingIndexes", typeof(object));
			this.ConnectionTypes = (byte[])info.GetValue("ConnectionTypes", typeof(object));
			this.NumberOfConnections = (byte)info.GetValue("NumberOfConnections", typeof(byte));
		}

		public Waypoint()
		{
		}

		public void Move(float x, float y, float z)
		{
            pos.X = x;
            pos.Y = y;
            pos.Z = z;
		}

		public bool Link(ushort destination, byte type)
		{
			if (NumberOfConnections == 10 || destination < 1)
				return false;

			for (ushort i = 0; i < NumberOfConnections; i++)
				if (SurroundingIndexes[i] == destination)
					return false;

			SurroundingIndexes[NumberOfConnections] = destination;
			ConnectionTypes[NumberOfConnections] = type;
			NumberOfConnections++;
			return true;
		}

        public void UnlinkAll()
        {
            SurroundingIndexes = new ushort[10];
            ConnectionTypes = new byte[10];
            NumberOfConnections = 0;
        }

		public void Unlink(uint destination)
		{
            for (int i = 0; i < NumberOfConnections; i++)
            { 
                if (SurroundingIndexes[i] == destination)
                {
                    for (int j = i; j < NumberOfConnections - 1; j++)
                    {
                        SurroundingIndexes[j] = SurroundingIndexes[j + 1];
                        ConnectionTypes[j] = ConnectionTypes[j + 1];
                    }
                    NumberOfConnections--;
                    break;
                }
            }
		}

	}
}