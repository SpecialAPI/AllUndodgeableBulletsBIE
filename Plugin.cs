using BepInEx;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AllUndodgeableBulletsBIE
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "spapi.etg.allundodgeablebullets";
        public const string NAME = "All Undodgeable Bullets";
        public const string VERSION = "1.0.2";
        public static bool UndodgeableMode;

        public static MethodInfo ub_pc_iif = AccessTools.Method(typeof(Plugin), nameof(UndodgeableBullets_PreCollision_IgnoreInvulnerableFrames));
        public static MethodInfo ub_hd_iif = AccessTools.Method(typeof(Plugin), nameof(UndodgeableBullets_HandleDamage_IgnoreInvulnerableFrames));
        public static MethodInfo ub_hd_iiffd = AccessTools.Method(typeof(Plugin), nameof(UndodgeableBullets_HandleDamage_IgnoreInvulnerableFramesForDamage));

        public void Awake()
        {
            ETGModConsole.CommandDescriptions["undodgeablemode"] = "Toggles All Undodgeable Bullets mode.";

            ETGModConsole.Commands.AddUnit("undodgeablemode", x =>
            {
                UndodgeableMode = !UndodgeableMode;
                if (!UndodgeableMode)
                {
                    ETGModConsole.Log("All Undodgeable Bullets mode disabled.");
                    return;
                }

                ETGModConsole.Log("All Undodgeable Bullets mode enabled.");

                var l = new List<string>()
                {
                    "Good luck beating dragun with that on.",
                    "You can go unbind your dodgeroll key now.",
                    "You're lucky that this doesn't work on beams.",
                    "Why?",
                    "Can you beat this mode? I don't think you can."
                };
                ETGModConsole.Log(BraveUtility.RandomElement(l));
            });

            new Harmony(GUID).PatchAll();
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.OnPreCollision))]
        [HarmonyILManipulator]
        public static void UndodgeableBullets_PreCollision_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchCallOrCallvirt<tk2dSpriteAnimator>(nameof(tk2dSpriteAnimator.QueryInvulnerabilityFrame)), 2))
                return;

            crs.Emit(OpCodes.Ldarg_3);
            crs.Emit(OpCodes.Call, ub_pc_iif);
        }

        public static bool UndodgeableBullets_PreCollision_IgnoreInvulnerableFrames(bool curr, SpeculativeRigidbody target)
        {
            if (UndodgeableMode && target != null && target.gameActor != null && target.gameActor is PlayerController)
                return false;

            return curr;
        }

        [HarmonyPatch(typeof(Projectile), nameof(Projectile.HandleDamage))]
        [HarmonyILManipulator]
        public static void UndodgeableBullets_HandleDamage_Transpiler(ILContext ctx)
        {
            var crs = new ILCursor(ctx);

            if (!crs.JumpToNext(x => x.MatchCallOrCallvirt<tk2dSpriteAnimator>(nameof(tk2dSpriteAnimator.QueryInvulnerabilityFrame))))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.Emit(OpCodes.Call, ub_hd_iif);

            if (!crs.JumpToNext(x => x.MatchLdcI4(0), 7))
                return;

            crs.Emit(OpCodes.Ldarg_1);
            crs.Emit(OpCodes.Call, ub_hd_iiffd);
        }

        public static bool UndodgeableBullets_HandleDamage_IgnoreInvulnerableFrames(bool curr, SpeculativeRigidbody target)
        {
            if(UndodgeableMode && target != null && target.gameActor != null && target.gameActor is PlayerController)
                return false;

            return curr;
        }

        public static bool UndodgeableBullets_HandleDamage_IgnoreInvulnerableFramesForDamage(bool curr, SpeculativeRigidbody target)
        {
            if (UndodgeableMode && target != null && target.gameActor != null && target.gameActor is PlayerController)
                return true;

            return curr;
        }
    }
}
