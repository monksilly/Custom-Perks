using System;
using System.Reflection;
using CustomPerks.Patches;
using UnityEngine;

namespace CustomPerks.PerkModules
{
    public class PerkModule_OnDamage : PerkModule
    {
        public string applyBuffOnHit;
        public float buffDuration = 3.0f;
        public float buffAmount = 1.0f;
        public float healAmount = 0f;
        public bool dropAllItems = false;
        public string damageTypeFilter = "any";
        
        private BuffContainer _currentBuff;
        private float _buffTimer = 0f;
        private Perk _perk;

        public override void Initialize(Perk p)
        {
            base.Initialize(p);
            _perk = p;
            ENT_Player_DamagePatch._OnDamage += OnDamageReceived;

            if (!string.IsNullOrEmpty(applyBuffOnHit))
            {
                _currentBuff = ENT_Player.GetPlayer().curBuffs.GetBuffContainer(applyBuffOnHit);
                if (_currentBuff == null)
                {
                    var buff = new BuffContainer { id = applyBuffOnHit };
                    ENT_Player.GetPlayer().curBuffs.AddBuff(buff);
                    _currentBuff = ENT_Player.GetPlayer().curBuffs.GetBuffContainer(applyBuffOnHit);
                }
                _currentBuff.SetMultiplier(0f);
            }
        }

        public override void Update()
        {
            base.Update();

            if (_currentBuff != null && _buffTimer > 0f)
            {
                _buffTimer -= Time.deltaTime;
                if (_buffTimer <= 0f)
                {
                    _currentBuff.SetMultiplier(0f);
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            ENT_Player_DamagePatch._OnDamage -= OnDamageReceived;
            
            if (_currentBuff != null && !string.IsNullOrEmpty(applyBuffOnHit))
            {
                ENT_Player.GetPlayer().curBuffs.RemoveBuffContainer(applyBuffOnHit);
            }
        }

        private void OnDamageReceived(float amount, string type)
        {
            if (damageTypeFilter != "any" && !type.ToLower().Contains(damageTypeFilter.ToLower()))
                return;

            if (_currentBuff != null && buffAmount > 0f)
            {
                _currentBuff.SetMultiplier(buffAmount * (float)_perk.GetStackAmount());
                _buffTimer = buffDuration;
            }

            if (healAmount > 0f)
            {
                ENT_Player.GetPlayer().health = Mathf.Min(
                    ENT_Player.GetPlayer().health + healAmount,
                    ENT_Player.GetPlayer().maxHealth
                );
            }
        }
    }
}

