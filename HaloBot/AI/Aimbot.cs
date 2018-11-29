using System;
using System.Timers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace HaloBot
{
    public class Aimbot
    {
        private const float RANDOMNESS_SCALE = 0.15F;
        private const int TICKS_FOR_NEW_RANDOM = 20;
        private const float PLAYER_HEIGHT = 0.62F;
        private const float LEAD_SCALE = 0.07F;
        private const float GRAVITY = 3.3F;

        private Timer aimTimer;
        private bool aiming;
        private int times;
        private bool navLocked;
        private bool lookAheadLocked;
        private Structures.FLOAT3 target;
        private Structures.FLOAT3 navTarget;
        private int targetPlayerIndex;
        private bool followPlayer;
        private bool smoothAiming;
        public bool arcMode;
        public float ProjectileVelocity;
        public float GravityScale;

        public Random rand; //also used by nav for strafing
        private float randVal;
        private MemoryReaderWriter gameState;

        public Aimbot(MemoryReaderWriter gameState)
        {
            this.gameState = gameState;
            navLocked = false;
            followPlayer = false;
            arcMode = false;
            ProjectileVelocity = 0;
            GravityScale = 1;

            rand = new Random();
            aimTimer = new Timer(10);
            aimTimer.Elapsed += new ElapsedEventHandler(aimTimer_Elapsed);
            aimTimer.Start();
        }

        #region Control

        public void Pause()
        {
            aiming = false;
        }

        public void Start()
        {
            aiming = true;
        }

        public void NavLock(Structures.FLOAT3 pos)
        {
            navTarget = pos;
            navLocked = true;
        }

        public void NavUnlock()
        {
            navLocked = false;
        }

        public bool NavLocked()
        {
            return navLocked;
        }

        public void LookAheadLock(Structures.FLOAT3 pos)
        {
            navTarget = pos;
            lookAheadLocked = true;
        }

        public void LookAheadUnlock()
        {
            lookAheadLocked = false;
        }

        public bool LookAheadLocked()
        {
            return lookAheadLocked;
        }

        public void SetTarget(int playerIndex, bool smooth)
        {
            if (navLocked)
                return;
            targetPlayerIndex = playerIndex;
            smoothAiming = smooth;
            followPlayer = true;
        }

        public void SetTarget(Structures.FLOAT3 pos, bool smooth)
        {
            if (navLocked)
                return;
            Debug.WriteLine(pos);
            target = pos;
            smoothAiming = smooth;
            followPlayer = false;
        }

        public int GetTargetIndex()
        {
            return targetPlayerIndex;
        }

        public Structures.FLOAT3 GetTargetPos()
        {
            return target;
        }

        #endregion


        void aimTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (navLocked || lookAheadLocked)
            {
                AimAt(navTarget);
                return;
            }

            if (!aiming)
                return;

            if (followPlayer)
                target = LeadPlayer(targetPlayerIndex, arcMode);

            if (arcMode)
            {
                Structures.FLOAT3 localPos = gameState.LocalPosition;
                float dx = target.X - localPos.X;
                float dy = target.Y - localPos.Y;
                float dz = target.Z - localPos.Z;

                float newH = (float)Math.Atan2(dy, dx);
                float newV = CalculateBallisticAngle(dx, dy, dz, ProjectileVelocity);

                if (!float.IsNaN(newV) && !float.IsNaN(newH))
                    AimAt(newH, newV, 0.0F);
            }
            else
            {
                AimAt(target);
                times++;
                if (times >= TICKS_FOR_NEW_RANDOM)
                {
                    randVal = (float)rand.NextDouble() - 0.5F;
                    times = 0;
                }
            }
        }

        private float CalculateBallisticAngle(float dx, float dy, float dz, float v)
        {
            //the formula approaches the right angle as gravity scale approaches 0, but
            //fails at 0, so use this special case
            if (GravityScale == 0)
                return (float)Math.Atan2(dz, Math.Sqrt(dx * dx + dy * dy));

            float x = Navigation.Distance2D(dx, dy);

            double root =
                (
                    v * v -
                    Math.Sqrt(
                                 v * v * v * v -
                                 GRAVITY * GravityScale * (GRAVITY * GravityScale * x * x + 2 * dz * v * v)
                             )
                )
                /
                (GRAVITY * GravityScale * x);

            if (root == double.NaN)
                root = 0;

            return (float)Math.Atan(root);
        }

        private void AimAt(Structures.FLOAT3 pos)
        {
            Structures.FLOAT3 localPos = gameState.LocalPosition;
            float dx = pos.X - localPos.X;
            float dy = pos.Y - localPos.Y;
            float dz = pos.Z - localPos.Z;

            float dist = Navigation.Distance3D(pos, localPos);

            float aH = (float)Math.Atan2(dy, dx);
            float aV = (float)Math.Atan2(dz, Math.Sqrt(dx * dx + dy * dy));

            AimAt(aH, aV, dist);
        }

        public float TargetClosenessToView(Structures.FLOAT3 pos)
        {
            float oldH = gameState.PlayerHorizontalViewAngle;
            float oldV = gameState.PlayerVerticalViewAngle;

            Structures.FLOAT3 localPos = gameState.LocalPosition;
            float dx = pos.X - localPos.X;
            float dy = pos.Y - localPos.Y;
            float dz = pos.Z - localPos.Z;

            float aH = (float)Math.Atan2(dy, dx);
            float aV = (float)Math.Atan2(dz, Math.Sqrt(dx * dx + dy * dy));

            return TargetClosenessToView(ref oldH, oldV, ref aH, aV);
        }

        public float TargetClosenessToView(ref float oldH, float oldV, ref float newH, float newV)
        {
            //fixes the angles so they average nicely
            newH = NormalizeAngle(newH);
            oldH = NormalizeAngle(oldH);
            if (newH > oldH && newH - oldH > Math.PI)
                newH -= (float)(Math.PI * 2);
            else if (newH < oldH && oldH - newH > Math.PI)
                newH += (float)(Math.PI * 2);

            //calculate sM = how closely we're aiming at the target already
            float sH = Math.Abs((newH - oldH) / (float)(Math.PI * 2));
            float sV = Math.Abs((newV - oldV) / (float)(Math.PI * 2));

            float sM = Math.Max(sH, sV);
            if (sM == float.NaN)
                sM = 0;

            return sM;
        }

        private void AimAt(float newH, float newV, float dist)
        {
            //performs no smoothing and adds no randomness
            if (!smoothAiming)
            {
                gameState.PlayerHorizontalViewAngle = newH;
                gameState.PlayerVerticalViewAngle = newV;
                return;
            }

            //introduce a bit of randomness to the new angles
            if (followPlayer && !arcMode)
            {
                newH += randVal * RANDOMNESS_SCALE * (1 / dist);
                newV += randVal * RANDOMNESS_SCALE * (1 / dist);
            }

            float oldH = gameState.PlayerHorizontalViewAngle;
            float oldV = gameState.PlayerVerticalViewAngle;
            float sM = TargetClosenessToView(ref oldH, oldV, ref newH, newV);

            //set stick = "stickiness" of aim to target as a function of sM, smooths aiming
            float stick;
            if (sM > 0.7)
                stick = 0.03F;
            else if (sM > 0.1)
                stick = 0.04F;
            else if (sM > 0.05)
                stick = 0.05F;
            else if (sM > 0.005)
                stick = 0.1F;
            else if (sM > 0.0005)
                stick = 0.5F;
            else
                stick = 0.8F;

            if (dist < 3)
                stick *= 2.5F;
            else if (dist < 8)
                stick *= 1.2F;
            else if (dist < 40)
                stick *= 1.0F;
            else if (dist < 50)
                stick *= 0.7F;
            else if (dist < 60)
                stick *= 0.6F;
            else if (dist < 70)
                stick *= 0.4F;
            else
                stick *= 0.3F;

            //weighted average of old and new angles
            gameState.PlayerHorizontalViewAngle = oldH * (1 - stick) + newH * stick;
            gameState.PlayerVerticalViewAngle = oldV * (1 - stick) + newV * stick;
        }

        private float NormalizeAngle(float a)
        {
            return (a %= (float)(Math.PI * 2)) >= 0 ? a : (a + (float)(Math.PI * 2));
        }

        public Structures.FLOAT3 LeadPlayer(int playerIndex, bool arcShot)
        {
            Structures.FLOAT3 pos = gameState.PlayerPosition(playerIndex);
            Structures.FLOAT3 vel = gameState.PlayerVelocity(playerIndex);
            Structures.FLOAT3 me = gameState.LocalPosition;
            int ping = gameState.LocalPing;
            float dist = Navigation.Distance2D(pos.X - me.X, pos.Y - me.Y);
            float travelTime = (ProjectileVelocity == 0) ? 0 : (float)(540 * (dist / ProjectileVelocity));

            //aim at their feet if arcshot enabled and also ignore vertical velocity (likely jumping)
            if (arcShot)
            {
                vel.Z = 0;
                pos.Z -= PLAYER_HEIGHT;
            }

            //account for ping
            pos.X += vel.X * (ping + travelTime) * LEAD_SCALE;
            pos.Y += vel.Y * (ping + travelTime) * LEAD_SCALE;
            pos.Z += vel.Z * (ping + travelTime) * LEAD_SCALE;

            return pos;
        }

    }
}
