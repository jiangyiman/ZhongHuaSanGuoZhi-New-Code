﻿namespace GameObjects.PersonDetail
{
    using GameObjects;
    using GameObjects.Conditions;
    using GameObjects.Influences;
    using GameObjects.TroopDetail;
    using System;
    using System.Collections.Generic;

    public class Title : GameObject
    {
        private bool combat;

        public ConditionTable Conditions = new ConditionTable();
        public ConditionTable LoseConditions = new ConditionTable(); //失去条件

        //public ConditionTable LoseArchitectureConditions = new ConditionTable(); //失去建筑条件
       // public ConditionTable LoseFactionConditions = new ConditionTable(); //失去势力条件

        public InfluenceTable Influences = new InfluenceTable();
        private TitleKind kind;
        private int level;

        public ConditionTable ArchitectureConditions = new ConditionTable();
        public ConditionTable FactionConditions = new ConditionTable();

        public bool ManualAward
        {
            get;
            set;
        }

        public int AutoLearn
        {
            get;
            set;
        }

        public string AutoLearnText
        {
            get;
            set;
        }

        public string AutoLearnTextByCourier
        {
            get;
            set;
        }

        public PersonList Persons = new PersonList();

        private int[] generationChance = new int[10];
        public int[] GenerationChance
        {
            get
            {
                return generationChance;
            }
        }
        public int RelatedAbility { get; set; }

        public int GetRelatedAbility(Person p)
        {
            switch (RelatedAbility)
            {
                case 0: return p.Strength;
                case 1: return p.Command;
                case 2: return p.Intelligence;
                case 3: return p.Politics;
                case 4: return p.Glamour;
            }
            return 0;
        }

        public int MapLimit
        {
            get;
            set;
        }

        public int FactionLimit
        {
            get;
            set;
        }

        public int InheritChance
        {
            get;
            set;
        }

        private bool? containsLeaderOnlyCache = null;
        public bool ContainsLeaderOnly
        {
            get
            {
                if (containsLeaderOnlyCache != null)
                {
                    return containsLeaderOnlyCache.Value;
                }
                foreach (Influence i in this.Influences.Influences.Values)
                {
                    if (i.Kind.ID == 281)
                    {
                        containsLeaderOnlyCache = true;
                        return true;
                    }
                }
                containsLeaderOnlyCache = false;
                return false;
            }
        }

        public MilitaryType MilitaryTypeOnly
        {
            get
            {
                foreach (Influence i in this.Influences.Influences.Values)
                {
                    if (i.Kind.ID == 290)
                    {
                        return (MilitaryType)Enum.Parse(typeof(MilitaryType), i.Parameter);
                    }
                }
                return MilitaryType.其他;
            }
        }


        public int MilitaryKindOnly
        {
            get
            {
                foreach (Influence i in this.Influences.Influences.Values)
                {
                    if (i.Kind.ID == 300)
                    {
                        return int.Parse(i.Parameter);
                    }
                }
                return -1;
            }
        }

        public void AddInfluence(Influence influence)
        {
            this.Influences.AddInfluence(influence);
        }

        public bool CanLearn(Person person)
        {
            return CanLearn(person, false);
        }

        public bool WillLose(Person person) //失去条件
        {
            foreach (Condition condition in this.LoseConditions.Conditions.Values)
            {
                if (!condition.CheckCondition(person))
                {
                    return false;
                }
            }

            return true;
        }

        public bool CheckLimit(Person person)
        {
             if (person.BelongedFaction != null && person.BelongedFaction.PersonCount > this.FactionLimit)
            {
                int cnt = 0;
                foreach (Person p in person.BelongedFaction.Persons)
                {
                    if (p.Titles.Contains(this))
                    {
                        cnt++;
                    }
                }
                if (cnt >= this.FactionLimit) return false;
            }
            if (base.Scenario.Persons.Count > this.MapLimit)
            {
                int cnt = 0;
                foreach (Person p in base.Scenario.Persons)
                {
                    if ((p.Alive || p.Available) && p.Titles.Contains(this))
                    {
                        cnt++;
                    }
                }
                if (cnt >= this.MapLimit) return false;
            }
            return true;
        }

        public bool CanLearn(Person person, bool ignoreAutoLearn)
        {
            if (AutoLearn > 0 && !ignoreAutoLearn) return false; 
            foreach (Condition condition in this.Conditions.Conditions.Values)
            {
                if (!condition.CheckCondition(person))
                {
                    return false;
                }
            }
            foreach (Condition condition in this.ArchitectureConditions.Conditions.Values)
            {
                if (person.LocationArchitecture == null) return false;
                if (!condition.CheckCondition(person.LocationArchitecture)) return false;
            }
            foreach (Condition condition in this.FactionConditions.Conditions.Values)
            {
                if (person.BelongedFaction == null) return false;
                if (!condition.CheckCondition(person.BelongedFaction)) return false;
            }
           
            return CheckLimit(person);
        }

        public bool CanBeChosenForGenerated()
        {
            foreach (Condition condition in this.Conditions.Conditions.Values)
            {
                if (condition.Kind.ID == 902) return false;
            }
            return true;
        }

        public bool CanBeBorn()
        {
            foreach (Condition condition in this.Conditions.Conditions.Values)
            {
                if (condition.Kind.ID == 901) return false;
            }
            return true;
        }

        public bool CanBeBorn(Person person)
        {
            foreach (Condition condition in this.Conditions.Conditions.Values)
            {
                if (condition.Kind.ID == 901) return false;
                if (new List<int> { 600, 610, 970, 971 }.Contains(condition.Kind.ID))
                {
                    if (!condition.CheckCondition(person))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public GameObjectList GetConditionList()
        {
            return this.Conditions.GetConditionList();
        }

        public GameObjectList GetInfluenceList()
        {
            return this.Influences.GetInfluenceList();
        }

        public bool Combat
        {
            get
            {
                return this.combat;
            }
            set
            {
                this.combat = value;
            }
        }

        public int ConditionCount
        {
            get
            {
                return this.Conditions.Count;
            }
        }

        private string DescriptionCache;
        public string Description
        {
            get
            {
                if (DescriptionCache == null)
                {
                    string str = "";
                    foreach (Influence influence in this.Influences.Influences.Values)
                    {
                        str = str + "•" + influence.Description;
                    }
                    DescriptionCache = str;
                }
                return DescriptionCache;
            }
        }

        public int InfluenceCount
        {
            get
            {
                return this.Influences.Count;
            }
        }

        public TitleKind Kind
        {
            get
            {
                return this.kind;
            }
            set
            {
                this.kind = value;
            }
        }

        public string KindName
        {
            get
            {
                return this.Kind.Name;
            }
        }

        public int Level
        {
            get
            {
                return this.level;
            }
            set
            {
                this.level = value;
            }
        }

        private int? merit = null;
        public int Merit
        {
            get
            {
                if (merit == null)
                {
                    merit = (int)(Math.Pow(Math.Max(1, AIPersonValue - 15) * 0.2828 + 1, 0.75) * 5);
                }
                return merit.Value;
            }
        }

        private int? fightingMerit = null;
        public int FightingMerit
        {
            get
            {
                if (fightingMerit == null)
                {
                    fightingMerit = (int)(Math.Pow(Math.Max(1, AIFightingPersonValue - 15) * 0.2828 + 1, 0.75) * 5);
                }
                return fightingMerit.Value;
            }
        }

        private int? subOfficerMerit = null;
        public int SubOfficerMerit
        {
            get
            {
                if (subOfficerMerit == null)
                {
                    subOfficerMerit = (int)(Math.Pow(Math.Max(1, AISubOfficerPersonValue - 15) * 0.2828 + 1, 0.75) * 5);
                }
                return subOfficerMerit.Value;
            }
        }

        public string Prerequisite
        {
            get
            {
                string str = "";
                foreach (Condition condition in this.Conditions.Conditions.Values)
                {
                    str = str + "•" + condition.Name;
                }
                foreach (Condition condition in this.ArchitectureConditions.Conditions.Values)
                {
                    str = str + "•" + condition.Name;
                }
                foreach (Condition condition in this.FactionConditions.Conditions.Values)
                {
                    str = str + "•" + condition.Name;
                }
                /*
                foreach (Condition condition in this.LoseConditions.Conditions.Values)
                {
                    str = str + "•" + condition.Name;
                }
                */
              
                return str;
            }
        }

        public string DetailedName
        {
            get
            {
                return this.Level + "级" + this.KindName + "「" + this.Name + "」";
            }
        }

        private double? aiPersonValue = null;
        public double AIPersonValue
        {
            get
            {
                if (aiPersonValue != null)
                {
                    return aiPersonValue.Value;
                }

                calculatePersonValues();
                return aiPersonValue.Value;
            }
        }

        private double? aiFightingPersonValue = null;
        public double AIFightingPersonValue
        {
            get
            {
                if (aiFightingPersonValue != null)
                {
                    return aiFightingPersonValue.Value;
                }

                calculatePersonValues();
                return aiFightingPersonValue.Value;
            }
        }

        private double? aiSubofficerPersonValue = null;
        public double AISubOfficerPersonValue
        {
            get
            {
                if (aiSubofficerPersonValue != null)
                {
                    return aiSubofficerPersonValue.Value;
                }

                calculatePersonValues();
                return aiSubofficerPersonValue.Value;
            }
        }

        private void calculatePersonValues()
        {
            double d = 1;
            bool hasKind = false;
            bool hasType = false;

            aiPersonValue = 0;
            aiFightingPersonValue = 0;
            aiSubofficerPersonValue = 0;
            bool leaderEffective = false;
            foreach (Influence i in this.Influences.GetInfluenceList())
            {
                switch (i.Kind.ID)
                {
                    case 281:
                        d *= 0.8;
                        leaderEffective = true;
                        break;
                    case 290:
                        if (hasKind)
                        {
                            d *= 1.2;
                        }
                        else
                        {
                            hasKind = true;
                            d *= 0.4;
                        }
                        break;
                    case 300:
                        if (hasType)
                        {
                            d *= 1.1;
                            if (d > 1)
                            {
                                d = 1;
                            }
                        }
                        else
                        {
                            hasKind = true;
                            d *= 0.2;
                        }
                        break;
                }
                aiPersonValue += i.AIPersonValue * d;
                if (i.Kind.Combat)
                {
                    aiFightingPersonValue += i.AIPersonValue * d;
                }
                if (!leaderEffective && i.Kind.Combat)
                {
                    aiSubofficerPersonValue += i.AIPersonValue * d;
                }
            }
        }

        private int? aiPersonLevel = null;
        public int AIPersonLevel
        {
            get
            {
                if (aiPersonLevel != null)
                {
                    return aiPersonLevel.Value;
                }
                aiPersonLevel = (int)(Math.Sqrt(Math.Max(1, AIPersonValue - 15)) * 0.2828 + 1);
                return aiPersonLevel.Value;
            }
        }

        public static Dictionary<TitleKind, List<Title>> GetKindTitleDictionary(GameScenario scen)
        {
            GameObjectList rawTitles = scen.GameCommonData.AllTitles.GetTitleList().GetRandomList();
            Dictionary<TitleKind, List<Title>> titles = new Dictionary<TitleKind, List<Title>>();
            foreach (Title t in rawTitles)
            {
                if (!titles.ContainsKey(t.Kind))
                {
                    titles[t.Kind] = new List<Title>();
                }
                titles[t.Kind].Add(t);
            }
            return titles;
        }
    }
}

