using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace CTC
{
    /// <summary>
    /// One of the skills the player has.
    /// These might be server-side configured in the future?
    /// </summary>
    public class ClientSkill
    {
        public String LongName;
        public int Value = 0;
        public int Percent = 0;

        public ClientSkill(String LongName)
        {
            this.LongName = LongName;
        }
    };

    /// <summary>
    /// 
    /// </summary>
    public class ClientPlayer : ClientCreature
    {
        public int AccountId { get; set; }


        public int PlayerId { get; set; }

        public string Name { get; set; }

        public int Level { get; set; } = 1;

        public long Balance { get; set; }
        public byte Blessings { get; set; }
        public int Cap { get; set; } = 400;
        public int Capacity { get; set; }
        public int Experience { get; set; }
        public int GroupId { get; set; } = 1;
        public int Health { get; set; } = 150;
        public int HealthMax { get; set; } = 150;
        public long LastLogin { get; set; }
        public long LastLogout { get; set; }
        public int LookAddons { get; set; }
        public int LookBody { get; set; }
        public int LookFeet { get; set; }
        public int LookHead { get; set; }
        public int LookLegs { get; set; }
        public int Mana { get; set; }
        public int ManaMax { get; set; }
        public long ManaSpent { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int PosZ { get; set; }
        public byte Save { get; set; } = 1;
        public int Sex { get; set; }
        public int SkillAxe { get; set; } = 10;
        public long SkillAxeTries { get; set; }
        public int SkillClub { get; set; } = 10;
        public long SkillClubTries { get; set; }
        public int SkillDist { get; set; } = 10;
        public long SkillDistTries { get; set; }
        public int SkillFishing { get; set; } = 10;
        public long SkillFishingTries { get; set; }
        public int SkillFist { get; set; } = 10;

        public Dictionary<string, ClientSkill> Skill = new Dictionary<string, ClientSkill>();

        //public int Capacity = Cap;
        //public int Experience = 0;
        public ClientSkill LevelSkill = new ClientSkill("Level");
        public ClientSkill MagicLevel = new ClientSkill("Magic Level");

        public ConditionState Conditions = ConditionState.None;

        public ClientPlayer(UInt32 PlayerId)
            : base(PlayerId)
        {
            Skill["SkillFist"] = new ClientSkill("Fist Fighting");
            Skill["SkillClub"] = new ClientSkill("Club Fighting");
            Skill["Sword"] = new ClientSkill("Sword Fighting");
            Skill["Axe"] = new ClientSkill("Axe Fighting");
            Skill["Dist"] = new ClientSkill("Distance Fighting");
            Skill["Shield"] = new ClientSkill("Shielding");
            Skill["Fish"] = new ClientSkill("Fishing");
        }
    }
}
