﻿using ChampionsOfForest.Effects;
using System;
using TheForest.Utils;
using UnityEngine;

namespace ChampionsOfForest.Player
{
    public static class SpellActions
    {
        public static float BlinkRange = 15;
        public static float BlinkDamage = 0;
        public static void DoBlink()
        {

            RaycastHit[] hits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, BlinkRange);
            foreach (RaycastHit hit in hits)
            {
                if (BlinkDamage != 0)
                {
                    if (hit.transform.root.CompareTag("enemyCollide"))
                    {
                        float dmg = BlinkDamage + ModdedPlayer.instance.SpellDamageBonus / 5;
                        dmg *= ModdedPlayer.instance.SpellAMP;
                        int dmgInt = Mathf.RoundToInt(dmg);
                        if (GameSetup.IsMpClient)
                        {
                            BoltEntity enemyEntity = hit.transform.root.GetComponent<BoltEntity>();
                            if (enemyEntity == null)
                            {
                                enemyEntity = hit.transform.root.GetComponentInChildren<BoltEntity>();
                            }

                            if (enemyEntity != null)
                            {
                                PlayerHitEnemy playerHitEnemy = PlayerHitEnemy.Create(enemyEntity);
                                playerHitEnemy.hitFallDown = true;
                                playerHitEnemy.Hit = dmgInt;
                                playerHitEnemy.Send();

                            }
                        }
                        else
                        {
                            hit.transform.SendMessageUpwards("Hit", dmgInt, SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                if (hit.transform.root != LocalPlayer.Transform.root && Vector3.Distance(hit.point, LocalPlayer.Transform.position) > 4)
                {
                    int tries = 0;
                    Vector3 hitPoint = hit.point;
                    while (Physics.Raycast(hitPoint, Vector3.up, 2f) && tries < 5)
                    {
                        hitPoint += -Camera.main.transform.forward;
                        tries++;
                    }
                    if (tries < 5)
                    {
                        BlinkTowards(hitPoint);
                        return;

                    }

                }
            }
            Vector3 checkPos = Camera.main.transform.position + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z).normalized * BlinkRange;
            if (Physics.Raycast(checkPos + Vector3.up * 2, Vector3.down, out RaycastHit hit1, 10f))
            {

                BlinkTowards(hit1.point + Vector3.up);
                return;
            }
            BlinkTowards(Camera.main.transform.position + Camera.main.transform.forward * (BlinkRange - 1));


        }
        private static void BlinkTowards(Vector3 point)
        {
            Vector3 vel = LocalPlayer.Rigidbody.velocity;
            LocalPlayer.Transform.root.position = point + Vector3.up;
            LocalPlayer.Rigidbody.velocity = vel * 1.5f;
        }





        public static bool HealingDomeGivesImmunity = false;
        public static void CreateHealingDome()
        {
            Vector3 pos = LocalPlayer.Transform.position;
            float radius = 8.5f;
            float healing = (ModdedPlayer.instance.LifeRegen + 13.5f) * ModdedPlayer.instance.SpellAMP * ModdedPlayer.instance.HealingMultipier;
            string immunity = "0;";
            if (HealingDomeGivesImmunity)
            {
                immunity = "1;";
            }
            float duration = 10;
            Network.NetworkManager.SendLine("SC2;" + Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" + radius + ";" + healing + ";" + immunity + duration + ";", Network.NetworkManager.Target.Everyone);
        }

        public static void BUFF_MultMS(float f)
        {
            ModdedPlayer.instance.MoveSpeed *= f;
        }
        public static void BUFF_DivideMS(float f)
        {
            ModdedPlayer.instance.MoveSpeed /= f;
        }

        public static void BUFF_MultAS(float f)
        {
            ModdedPlayer.instance.AttackSpeedMult *= f;
        }
        public static void BUFF_DivideAS(float f)
        {
            ModdedPlayer.instance.AttackSpeedMult /= f;
        }
        #region FLARE

        public static float FlareDamage = 10;
        public static float FlareSlow = 0.5f;
        public static float FlareBoost = 1.35f;
        public static float FlareHeal = 5;
        public static float FlareRadius = 4.5f;
        public static float FlareDuration = 15;
        public static void CastFlare()
        {
            Vector3 dir = LocalPlayer.Transform.position;
            float dmg = FlareDamage + ModdedPlayer.instance.SpellDamageBonus / 3;
            dmg *= ModdedPlayer.instance.SpellAMP;
            float slow = FlareSlow;
            float boost = FlareBoost;
            float duration = FlareDuration;
            float radius = FlareRadius;
            float Healing = FlareHeal + ModdedPlayer.instance.SpellDamageBonus / 20 + (ModdedPlayer.instance.LifeRegen / 1.2f) * ModdedPlayer.instance.HealthRegenPercent;
            Healing *= ModdedPlayer.instance.SpellAMP;

            Network.NetworkManager.SendLine("SC3;" + dir.x + ";" + dir.y + ";" + dir.z + ";" + "f;" + dmg + ";" + Healing + ";" + slow + ";" + boost + ";" + duration + ";" + radius + ";", Network.NetworkManager.Target.Everyone);
        }
        #endregion
        #region BLACK HOLE
        public static float BLACKHOLE_damage = 40;
        public static float BLACKHOLE_duration = 9;
        public static float BLACKHOLE_radius = 12;
        public static float BLACKHOLE_pullforce = 10;
        public static void CreatePlayerBlackHole()
        {
            float damage = (BLACKHOLE_damage + ModdedPlayer.instance.SpellDamageBonus / 7) * ModdedPlayer.instance.SpellAMP;
            RaycastHit[] hits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, 100);
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.root != LocalPlayer.Transform.root)
                {
                    Network.NetworkManager.SendLine("SC1;" + Math.Round(hits[i].point.x, 5) + ";" + Math.Round(hits[i].point.y, 5) + ";" + Math.Round(hits[i].point.z, 5) + ";" +
                        "f;" + damage + ";" + BLACKHOLE_duration + ";" + BLACKHOLE_radius + ";" + BLACKHOLE_pullforce + ";", Network.NetworkManager.Target.Everyone);
                    return;
                }
            }
            Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 50;
            Network.NetworkManager.SendLine("SC1;" + Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" +
                       "f;" + damage + ";" + BLACKHOLE_duration + ";" + BLACKHOLE_radius + ";" + BLACKHOLE_pullforce + ";", Network.NetworkManager.Target.Everyone);
        }
        #endregion

        #region SustainShield
        public static float ShieldPerSecond = 4;
        public static float MaxShield = 40;
        public static float ShieldCastTime;
        public static float ShieldPersistanceLifetime = 40;
        public static void CastSustainShieldActive()
        {
            float max = MaxShield + ModdedPlayer.instance.SpellDamageBonus / 2;
            max *= ModdedPlayer.instance.SpellAMP;
            float gain = ShieldPerSecond + ModdedPlayer.instance.SpellDamageBonus / 20;
            gain *= ModdedPlayer.instance.SpellAMP;
            ModdedPlayer.instance.damageAbsorbAmounts[1] = Mathf.Clamp(ModdedPlayer.instance.damageAbsorbAmounts[1] + Time.deltaTime * gain, 0, max);
            ShieldCastTime = Time.time;
        }
        public static void CastSustainShielPassive(bool on)
        {
            if (!on)
            {
                return;
            }

            if (ModdedPlayer.instance.damageAbsorbAmounts[1] > 0)
            {
                if (ShieldCastTime + ShieldPersistanceLifetime < Time.time)
                {
                    float loss = Time.deltaTime * (ShieldPerSecond + ModdedPlayer.instance.SpellDamageBonus / 5) * 5 * ModdedPlayer.instance.SpellDamageBonus;
                    ModdedPlayer.instance.damageAbsorbAmounts[1] = Mathf.Max(0, ModdedPlayer.instance.damageAbsorbAmounts[1] - loss);
                }
            }
        }
        #endregion


        #region WarCry
        public static float WarCryRadius = 50;
        public static bool WarCryGiveDamage = false;
        public static bool WarCryGiveArmor = false;
        public static int WarCryArmor => ModdedPlayer.instance.Armor / 10;
        public static void CastWarCry()
        {
            WarCry.GiveEffect(WarCryGiveDamage, WarCryGiveArmor, WarCryArmor);
            WarCry.SpawnEffect(LocalPlayer.Transform.position, WarCryRadius);
            if (BoltNetwork.isRunning)
            {
                Vector3 pos = LocalPlayer.Transform.position;
                string s = "SC5;" + Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" + WarCryRadius + ";";
                if (WarCryGiveDamage)
                {
                    s += "t;";
                }
                else
                {
                    s += "f;";
                }

                if (WarCryGiveArmor)
                {
                    s += "t;" + WarCryArmor;
                }
                else
                {
                    s += "f;";
                }

                Network.NetworkManager.SendLine(s, Network.NetworkManager.Target.Others);
            }
        }

        #endregion
        public static float PortalDuration = 30;
        public static void CastPortal()
        {
            Vector3 pos = LocalPlayer.Transform.position + LocalPlayer.Transform.forward * 6;
            int id = Portal.GetPortalID();
            try
            {
                Portal.CreatePortal(pos, PortalDuration, id, LocalPlayer.IsInCaves, LocalPlayer.IsInEndgame);

            }
            catch (Exception e)
            {
                ModAPI.Log.Write(e.ToString());

            }

            if (BoltNetwork.isRunning)
            {
                Portal.SyncTransform(pos, PortalDuration, id, LocalPlayer.IsInCaves, LocalPlayer.IsInEndgame);
            }
        }


        public static bool MagicArrowDmgDebuff = false;
        public static bool MagicArrowDoubleSlow = false;
        public static float MagicArrowDuration = 15f;
        public static void CastMagicArrow()
        {
            float damage = 55 + ModdedPlayer.instance.SpellDamageBonus * 1.3f;
            damage = damage * ModdedPlayer.instance.SpellAMP;
            Vector3 pos = Camera.main.transform.position;
            Vector3 dir = Camera.main.transform.forward;
            if (GameSetup.IsSinglePlayer || GameSetup.IsMpServer)
            {
                MagicArrow.Create(pos, dir, damage, ModReferences.ThisPlayerID, MagicArrowDuration, MagicArrowDoubleSlow, MagicArrowDmgDebuff);
                if (BoltNetwork.isRunning)
                {
                    string s = "SC7;" + Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" + Math.Round(dir.x, 5) + ";" + Math.Round(dir.y, 5) + ";" + Math.Round(dir.z, 5) + ";";
                    s += damage + ";" + ModReferences.ThisPlayerID + ";" + MagicArrowDuration + ";";
                    if (MagicArrowDoubleSlow)
                    {
                        s += "t;";
                    }
                    else
                    {
                        s += "f;";
                    }

                    if (MagicArrowDmgDebuff)
                    {
                        s += "t;";
                    }
                    else
                    {
                        s += "f;";
                    }

                    Network.NetworkManager.SendLine(s, Network.NetworkManager.Target.Others);
                }
            }
            else if (GameSetup.IsMpClient)
            {
                MagicArrow.CreateEffect(pos, dir, MagicArrowDmgDebuff, MagicArrowDuration);
                string s = "SC7;" + Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" + Math.Round(dir.x, 5) + ";" + Math.Round(dir.y, 5) + ";" + Math.Round(dir.z, 5) + ";";
                s += damage + ";" + ModReferences.ThisPlayerID + ";" + MagicArrowDuration + ";";
                if (MagicArrowDoubleSlow)
                {
                    s += "t;";
                }
                else
                {
                    s += "f;";
                }

                if (MagicArrowDmgDebuff)
                {
                    s += "t;";
                }
                else
                {
                    s += "f;";
                }

                Network.NetworkManager.SendLine(s, Network.NetworkManager.Target.Others);
            }

        }


        public static void ToggleMultishot()
        {
            Multishot.IsOn = !Multishot.IsOn;
            Multishot.localPlayerInstance.SetActive(Multishot.IsOn);
        }


        public static float PurgeRadius = 14;
        public static bool PurgeHeal = false;
        public static void CastPurge()
        {
            Vector3 pos = LocalPlayer.Transform.position;

            Purge.Cast(pos, PurgeRadius,PurgeHeal);

            if (BoltNetwork.isRunning)
            {
                string s = "SC8;" + Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" + PurgeRadius+";" + (PurgeHeal?"1;":"0;");
                Network.NetworkManager.SendLine(s, Network.NetworkManager.Target.Others);
            }
        }

        public static float SnapFreezeDist = 22;
        public static float SnapFloatAmount = 0.1f;
        public static float SnapFreezeDuration = 20f;
        public static void CastSnapFreeze()
        {
            Vector3 pos = LocalPlayer.Transform.position;
            float dmg = 23 + ModdedPlayer.instance.SpellDamageBonus;
            dmg *= ModdedPlayer.instance.SpellAMP;
            string s = "SC9;" + Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" + SnapFreezeDist + ";" + SnapFloatAmount + ";" + SnapFreezeDuration + ";" + dmg + ";";
            Network.NetworkManager.SendLine(s, Network.NetworkManager.Target.Everyone);
        }

        public static float BL_Damage = 150;
        public static void CastBallLightning()
        {
            float dmg = BL_Damage + (4 * ModdedPlayer.instance.SpellDamageBonus);
            dmg *= ModdedPlayer.instance.SpellAMP;


            Vector3 pos = LocalPlayer.Transform.position + LocalPlayer.Transform.forward;
            Vector3 speed = Camera.main.transform.forward;

            speed.y = 0;
            speed.Normalize();
            speed *= 3;

            string s = "SC10;" +
                Math.Round(pos.x, 5) + ";" + Math.Round(pos.y, 5) + ";" + Math.Round(pos.z, 5) + ";" +
                Math.Round(speed.x, 5) + ";" + Math.Round(speed.y, 5) + ";" + Math.Round(speed.z, 5) + ";" +
                dmg + ";" + (BallLightning.lastID+1)+";";
            Network.NetworkManager.SendLine(s, Network.NetworkManager.Target.Everyone);

        }


        #region Bash
        public static float BashExtraDamage = 1.06f;
        public static float BashDamageBuff = 1f;
        public static float BashSlowAmount = 0.2f;
        public static float BashLifesteal = 0.0f;
        public static bool BashEnabled = false;
        public static float BashBleedChance = 0;
        public static float BashBleedDmg = 0.2f;
        public static float BashDuration = 3;

        public static void BashPassiveEnabled(bool on)
        {
            BashEnabled = on;
            //SpellDataBase.spellDictionary[17].icon = on ? Res.ResourceLoader.GetTexture(132) : Res.ResourceLoader.GetTexture(131);
        }
        public static void Bash(EnemyProgression ep, float dmg)
        {
            if (BashEnabled)
            {
                int id = 43;
                ep.Slow(id, BashSlowAmount, BashDuration);
                ep.DmgTakenDebuff(id, BashExtraDamage, BashDuration);
                if (BashBleedChance > 0 && UnityEngine.Random.value < BashBleedChance) ep.DoDoT((int)(dmg * BashBleedDmg), BashDuration);
                if (BashLifesteal > 0) LocalPlayer.Stats.HealthTarget += dmg * BashLifesteal;
            }

        }
        public static void Bash(ulong enemy, float dmg)
        {
            if (BashEnabled)
            {
                int id = 44+ModReferences.Players.IndexOf(LocalPlayer.GameObject);
                string s = "AN" + enemy + ";" + BashDuration + ";" + id + ";" + BashSlowAmount + ";" + BashExtraDamage + ";" + ((int)(dmg * BashBleedDmg)) + ";" + BashBleedChance + ";";
                Network.NetworkManager.SendLine(s, Network.NetworkManager.Target.OnlyServer);
                if (BashLifesteal > 0) LocalPlayer.Stats.HealthTarget += dmg * BashLifesteal;
            }

        }
        #endregion

        #region Frenzy
        public static Transform frenzytarget;
        public static int FrenzyMaxStacks= 5, FrenzyStacks= 0;
        public static float FrenzyAtkSpeed = 0, FrenzyDmg = 0.05f;
        public static bool Frenzy;
        public static void OnFrenzyAttack()
        {
            if (Frenzy)
            {
                FrenzyStacks++;
                FrenzyStacks = Mathf.Min(FrenzyMaxStacks, FrenzyStacks);

                BuffDB.AddBuff(19, 60, FrenzyStacks, 3);
            }
        }
        #endregion

        #region Focus
        public static float FocusBonusDmg, FocusOnHS = 1,FocusOnBS = 0.2f, FocusOnAtkSpeed = 1.3f,FocusSlowAmount = 0.8f,FocusSlowDuration =10;
        public static bool Focus;
        
        public static float FocusOnBodyShot()
        {
            CotfUtils.Log("BODY SHOT " + Focus);
            if (!Focus) return 1;
            if (FocusBonusDmg == 0)
            {
                FocusBonusDmg = FocusOnBS;
                BuffDB.AddBuff(14, 61, FocusOnAtkSpeed, 4f);
                return 1;
            }
            else
            {
                var result = 1f + FocusBonusDmg;
                FocusBonusDmg = 0;
                return result;
            }
        }
        public static float FocusOnHeadShot()
        {
            CotfUtils.Log("HEAD SHOT " + Focus);
            if (!Focus) return 1;
            if (FocusBonusDmg == 0)
            {
                FocusBonusDmg = FocusOnHS;
                return 1;
            }
            else
            {
                var result = 1f + FocusBonusDmg;
                FocusBonusDmg = 0;
                return result;
            }
        }
        #endregion

        #region SeekingArrow
        public static Transform SeekingArrow_Target;
        public static bool SeekingArrow;
        public static bool SeekingArrow_ChangeTargetOnHit;
        public static float SeekingArrow_TimeStamp,SeekingArrow_SlowDuration = 8,SeekingArrow_SlowAmount = 0.4f,SeekingArrow_DamagePerDistance = 0.01f;
        public static void SeekingArrow_Initialize()
        {
            SeekingArrow_Target =new GameObject().transform;
            SeekingArrow_Target.gameObject.AddComponent<SeekingArrow>();
            //some more visuals
        }
        public static void SeekingArrow_Active()
        {
            if (SeekingArrow_Target == null) SeekingArrow_Initialize();
            SeekingArrow_Target.transform.parent = null;
            SeekingArrow_Target.gameObject.SetActive(false);
            SeekingArrow = false;
            SeekingArrow_TimeStamp = 0;
            SeekingArrow_ChangeTargetOnHit = true;

        }

        public static void SeekingArrow_SetTarget(Transform t)
        {

        }
        public static void SeekingArrow_End()
        {
            SeekingArrow_Target.gameObject.SetActive(false);

        }
        #endregion


    }
   
}
