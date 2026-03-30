using System;
using System.Collections.Generic;
using System.Text;

namespace RPGCommon
{
        public enum JobType
        {
            Normal = 0,
            Stamina = 1,
            Speed = 2
        }


    public class JobData
    {
        public int MaxHp { get; set; }
        public float AttackSpeed { get; set; }
        public int MoveStack { get; set; }

        public float MoveStackRegen { get; set; }

        public JobData(int maxHp, float attackSpeed, int moveStack, float moveStackRegen)
        {
            MaxHp = maxHp;
            AttackSpeed = attackSpeed;
            MoveStack = moveStack;
            MoveStackRegen = moveStackRegen;
        }
    }
}
