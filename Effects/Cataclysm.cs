﻿using ChampionsOfForest.Player;
using System;
using TheForest.Utils;
using UnityEngine;

namespace ChampionsOfForest.Effects
{
    public class Cataclysm : MonoBehaviour
    {
        //static elements

        public enum TornadoType { Fire, Arcane }
        public static GameObject fireprefab;
        public static GameObject arcanaprefab;
        public static void AssignPrefabs()
        {
            try
            {

                AssetBundle bundle = Res.ResourceLoader.GetAssetBundle(2003);
                arcanaprefab = bundle.LoadAsset<GameObject>("FireColumnArcane.prefab");
                fireprefab = bundle.LoadAsset<GameObject>("FireColumn.prefab");
            }
            catch (Exception e)
            {

                CotfUtils.Log("Assign Prefabs error " + e.Message);
            }
        }

        public static void Create(Vector3 position, float radius, float damage, float duration, TornadoType tornadoType, bool isFromEnemy)
        {
            try
            {
                GameObject go = tornadoType == TornadoType.Fire ? GameObject.Instantiate(fireprefab) : GameObject.Instantiate(arcanaprefab);
                //float scale = radius / 18.5f;
                radius = 18.5f;
                go.transform.position = position + Vector3.down * 2;
                go.transform.rotation = Quaternion.identity;
                //go.transform.localScale = Vector3.one * scale * ScaleMult;
                Cataclysm c = go.AddComponent<Cataclysm>();
                c.damage = damage;
                c.radius = radius;
                c.duration = duration;
                c.isFromEnemy = isFromEnemy;
                c.isArcane = tornadoType == TornadoType.Arcane;
            }
            catch (Exception e)
            {

                CotfUtils.Log("Creating cataclym error " + e.Message);
            }
        }


        //cataclysm behavior
        private float damage;
        private float radius;
        private float duration;
        private bool isFromEnemy;
        private bool isArcane;
        private bool warmUpDone;
        private float lastHitTime;
        private const float HitFrequency = 0.33333333f;
        private Animator animator;
        private GameObject particleParent;

        private void Start()
        {
            try
            {
                warmUpDone = false;
                animator = transform.GetChild(0).GetChild(0).GetComponent<Animator>();
                animator.Play("Intro");
                particleParent = transform.GetChild(1).gameObject;
                particleParent.SetActive(false);
                Invoke("EnableParticles", 2);
                CotfUtils.Log("start cataclysm");
            }
            catch (Exception e)
            {

                CotfUtils.Log("Start cataclysm error " + e.Message);
            }
        }

        private void EnableParticles()
        {
            try
            {
                particleParent.SetActive(true);
                warmUpDone = true;
                Invoke("End", duration);
                CotfUtils.Log("enabled particles cataclysm");
            }
            catch (Exception e)
            {

                CotfUtils.Log("Enabling particles cataclysm error " + e.Message);
            }
        }

        private void Update()
        {
            particleParent.transform.Rotate(Vector3.up * Time.deltaTime * 45);
            if (warmUpDone)
            {
                if (lastHitTime + HitFrequency < Time.time&&!GameSetup.IsMpClient)
                {
                    if (isFromEnemy)
                    {
                        SendHitFromEnemy();
                    }
                    else
                    {
                        SendHitFromPlayer();
                    }

                    lastHitTime = Time.time;

                }
            }
        }

        private void SendHitFromPlayer()
        {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, Vector3.one, radius);
            int dmg = (int)damage;
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform.CompareTag("enemyCollide"))
                {
                    EnemyProgression ep = hits[i].transform.gameObject.GetComponentInParent<EnemyProgression>();
                    
                    if (ep != null)
                    {
                        if (isArcane)
                        {
                            ep.HitMagic(dmg);
                            ep.Slow(141, 0.0f, 2);
                            ep.DmgTakenDebuff(140, 1.5f, 7);

                        }
                        else
                        {
                            ep.HitMagic(dmg / 2);
                            ep.Slow(140, 0.5f, 7);
                            ep.SendMessage("Burn", SendMessageOptions.DontRequireReceiver);
                            ep.FireDebuff(140, dmg / 2, 15);

                        }
                    }
                    else
                    {
                        hits[i].transform.SendMessageUpwards("HitMagic", dmg, SendMessageOptions.DontRequireReceiver);

                    }
                }
            }
        }

        private void SendHitFromEnemy()
        {
            float sqrMagnitude = (LocalPlayer.Transform.position - transform.position).sqrMagnitude;
            if (sqrMagnitude < radius * radius)
            {
                int dmg = (int)(damage * (1 - ModdedPlayer.instance.MagicResistance));

                if (isArcane)
                {
                    //negative armor
                    BuffDB.AddBuff(21, 63, -dmg * 2, 30);
                    BuffDB.AddBuff(1, 64, 0.3f, 12);
                    BuffDB.AddBuff(2, 65, 0.3f, 12);
                    dmg *= 2;
                }
                else
                {
                    BuffDB.AddBuff(1, 64, 0.7f, 6);
                    BuffDB.AddBuff(2, 65, 0.7f, 6);
                    LocalPlayer.Stats.Burn();
                }
                LocalPlayer.Stats.Hit((int)(dmg * (1 - ModdedPlayer.instance.MagicResistance)), false, PlayerStats.DamageType.Drowning);
            }
        }

        private void End()
        {
            try
            {
                CotfUtils.Log("end cataclysm");

                warmUpDone = false;
                for (int i = 0; i < particleParent.transform.childCount; i++)
                {
                    particleParent.transform.GetChild(i).SendMessage("Stop", new object[] { true, ParticleSystemStopBehavior.StopEmitting });
                }
                animator.SetBool("Done", true);
                Destroy(gameObject, 3);
            }
            catch (Exception e)
            {

                CotfUtils.Log("End cataclysm error " + e.Message);
            }
        }
    }
}