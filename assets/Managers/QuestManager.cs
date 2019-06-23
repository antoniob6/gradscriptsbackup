/*this class abstracts most of the opperations with the quest
 * and internally does all the calculations to make valid quests that can be used directly
 */
using System.Collections.Generic;
using UnityEngine;
using sysRand = System.Random;
public class QuestManager : MonoBehaviour {


    QuestTypes[] allAvailableQuestTypes = { QuestTypes.location, QuestTypes.collect, QuestTypes.kill, QuestTypes.survive,
            QuestTypes.touch, QuestTypes.guard, QuestTypes.boss,QuestTypes.race,QuestTypes.jump};



    bool[,] compatabilityTable ;
    private void Start() {

        setCompatabilityTable();

    }

    private void setCompatabilityTable() {
        //Debug.Log("number of quest types "+(int)QuestTypes.NUMOFELEMENTS);
        compatabilityTable = new bool[(int)QuestTypes.NUMOFELEMENTS-1, (int)QuestTypes.NUMOFELEMENTS - 1];


        for (int i = 0; i < compatabilityTable.GetLength(0); i++) {
            for (int j = 0; j < compatabilityTable.GetLength(1); j++) {
                if (i == j)
                    compatabilityTable[i, j] = false;//quest is not compatable with itself
                else
                    compatabilityTable[i, j] = true;
            }
        }




    }




    //note that NOT quest is allways allowed
    //recursvie means that it allows AND quest and OR quest otherwize it'll repeat recursvely
    int playerCount;
    int questindex = -1;
    
    public Quest createRandomQuest(List<GameObject> players, GameManager GM,
        bool allowAND = true,QuestTypes[] usedQuests=null) {

        playerCount = players.Count;

        //return new SurviveQuest(players, GM);
        //return new GuardQuest(players, GM);
        //return new FollowQuest(players, GM);
        //return new BossQuest(players, GM);
        if (allowAND) {
           // questindex = 0;
            //return new CompoundQuest(players, GM);

            //return new ANDQuest(players, GM);
            //return new NOTQuest( new KillQuest(players, GM) );

        }
        if (questindex == 0) {
            questindex++;
            return new GuardQuest(players, GM);
        }else if(questindex == 1) {
            questindex++;
            return new CollectQuest(players, GM);
        } else if (questindex == 2) {
            questindex++;
            return new SurviveQuest(players, GM);
        }



        float rand = (float)new sysRand().NextDouble();
        bool choseANDORQuest = true;
        if (rand < 0.2f) 
            choseANDORQuest = false;//low chance of atomic quest (20%)
        
        if (allowAND && choseANDORQuest) {//the quest should be AND/OR/Compound
            //Debug.Log("creating random AND/OR/Compound quest");
            int randType = Random.Range(0, 3);

            if (randType == 0) {
                return new ANDQuest(players, GM);
            }else if (randType == 1) {
                
                return new ORQuest(players, GM);
            } else if (randType == 2) {
                
                return new CompoundQuest(players, GM);
            } else {
                Debug.Log("overflow in chosing AND/OR/CompoundQuest");
            }
        } else {//can be anything other than (AND/OR/Compound)
                //Debug.Log("creating random atomic quest");

            QuestTypes[] availableTypes = findAvailableTypes(usedQuests);//available quests other than AND/OR/NOT/Compound
            string typesS = "";
            foreach (QuestTypes qt in availableTypes) {
                typesS += "" + qt + ", ";
            }
            //Debug.Log("available types: " + typesS);
            //int randIndex = Random.Range(0, availableTypes.Length);
            int randIndex =new sysRand().Next(0,availableTypes.Length);
            QuestTypes currQuestType = availableTypes[randIndex];

            Quest chosenQuest= generateQuestByType(players,GM, currQuestType);

            //rand = Random.Range(0f, 1f);

            rand = (float)new sysRand().NextDouble();
            //Debug.Log("rand: "+rand);
            if (rand < 0.2f) {//make a not quest
                chosenQuest=new NOTQuest(chosenQuest);
            }

            


            return chosenQuest;
        }
        //if execution reaches this point then we wern't able to create the quest 

        Debug.Log("quest creation by table failed, returning to default behaviur");


        Quest createdQuest = null;
        //createdQuest = new LocationQuest(players, GM);
        //createdQuest = new CollectQuest(players, GM);
        //createdQuest = new KillQuest(players, GM);
        createdQuest = new BossQuest(players, GM);
        /*
                int randomInt = Random.Range(0, 5);

                switch (randomInt) {
                    case 0:
                        createdQuest = new LocationQuest(players,GM);
                        break;
                    case 1:
                        createdQuest = new KillQuest(players, GM);
                        break;
                    case 2:
                        createdQuest = new SurviveQuest(players, GM);
                        break;
                    case 3:
                        createdQuest = new CondQuest(players, GM);
                        break;
                    case 4:
                        createdQuest = new ANDQuest(players, GM);
                        break;
                    case 5:
                        createdQuest = new ORQuest(players, GM);
                        break;

                }
                */
        if (createdQuest==null)
            Debug.Log("couldn't create quest, returning null quest");
        return createdQuest;


    }

    private QuestTypes[] findAvailableTypes(QuestTypes[] usedQuests) {

        List<QuestTypes> qts = new List<QuestTypes>();
        qts.AddRange(allAvailableQuestTypes);

        MapManager MM = FindObjectOfType<MapManager>();
        if (MM&& MM.finishedCreatingPlatforms) {
            if (MM.isPlatformsOnly) { //remove boss quest because for platform only map
                if (qts.Contains(QuestTypes.boss))
                    qts.Remove(QuestTypes.boss);
                if (qts.Contains(QuestTypes.guard))
                    qts.Remove(QuestTypes.guard);
            }
        }


        if (playerCount == 1 && qts.IndexOf(QuestTypes.touch) >= 0) {//remove touch goal if we only have one player
            qts.Remove(QuestTypes.touch);
          //  Debug.Log("removing touch goal because we only have one player");
        }

        if (usedQuests == null) {
            //Debug.Log("empty array recieved");
            return qts.ToArray();
        }


        foreach(QuestTypes qt in usedQuests) {
            if (qt == QuestTypes.DEFAULT) {
                //Debug.Log("default quest found");
                continue;
            }
            for(int j = 0; j < compatabilityTable.GetLength(1); j++) {
                if (!compatabilityTable[(int)qt, j]) {
                    if (qts.IndexOf((QuestTypes)j) >= 0) {//an incompatable quest is found
                        qts.Remove((QuestTypes)j);
                    }
                }
            }
        }

        if (qts.Count <= 0) {   //fail safe system, so that we don't return empty list
            Debug.Log("all quests canceled each other");
            return allAvailableQuestTypes;
        }




        //Debug.Log("before sort: " +qts);
        qts.Sort();
        //Debug.Log("after sort:" +qts);

        return qts.ToArray();
    }

    /*
private Quest generateQuest() {

   if (players.Count >= 1) {

       if (currentRound == 0) {
           return new SurviveQuest(players, center, 100, "survive the enemies", this);
           //return new LocationQuest(players, center, 100, "find a piece of  candy", this, 2, 20);
           //return new CondQuest(players,1111,"get a higher score",this,0,0);
           //return new KillQuest(players, 100, "kill another player", this);
       } else if (currentRound == 1) {
           return new LocationQuest(players, center, 100, "find the things", this, 2, 20);
       } else if (currentRound == 2) {
           return new SurviveQuest(players, center, 100, "survive the enemies", this);

       } else {
           Quest q1 = new KillQuest(players, 100, "kill another player", this);
           Quest q2 = new LocationQuest(players, center, 100, "find the things", this, 2, 100);
           Quest q3 = new SurviveQuest(players, center, 100, "survive the enemies", this, questTimeLimit);
           List<Quest> ql = new List<Quest>();
           ql.Add(q1);
           ql.Add(q2);
           ql.Add(q3);
           return new CompoundQuest(ql);
       }


   } else {
       Debug.Log("need at least two players");
       return null;
   }

}
*/




    public enum QuestTypes
    {
        DEFAULT,location,collect,kill,survive,guard,boss,touch,jump,race,cond,and,or,compound,not,NUMOFELEMENTS
    }

    public QuestTypes getQuestType(Quest quest) {
        if(quest.GetType() == typeof(NOTQuest)) {
            quest = ((NOTQuest)quest).quest1;
        }
            
        if (quest.GetType() == typeof(LocationQuest)) {
            return QuestTypes.location;
        }
        if (quest.GetType() == typeof(CollectQuest)) {
            return QuestTypes.collect;
        }
        if (quest.GetType() == typeof(KillQuest)) {
            return QuestTypes.kill;
        }
        if (quest.GetType() == typeof(SurviveQuest)) {
            return QuestTypes.survive;
        }
        if (quest.GetType() == typeof(FollowQuest)) {
            return QuestTypes.touch;
        }
        if (quest.GetType() == typeof(GuardQuest)) {
            return QuestTypes.guard;
        }
        if (quest.GetType() == typeof(BossQuest)) 
            return QuestTypes.boss;
        if (quest.GetType() == typeof(JumpQuest))
            return QuestTypes.jump;
        if (quest.GetType() == typeof(RacingQuest))
            return QuestTypes.race;



        Debug.Log("couldn't find matching type: " + quest.GetType());
        return QuestTypes.DEFAULT;
    }



    private Quest generateQuestByType(List<GameObject> players, GameManager GM, QuestTypes currQuestType) {
        //Debug.Log("screening proccess worked, returning an atomic quest: " + currQuestType);
        if (currQuestType == QuestTypes.location)
            return new LocationQuest(players, GM);
        if (currQuestType == QuestTypes.collect)
            return new CollectQuest(players, GM);
        if (currQuestType == QuestTypes.kill)
            return new KillQuest(players, GM);
        if (currQuestType == QuestTypes.survive)
            return new SurviveQuest(players, GM);
        if (currQuestType == QuestTypes.touch)
            return new FollowQuest(players, GM);
        if (currQuestType == QuestTypes.guard)
            return new GuardQuest(players, GM);
        if (currQuestType == QuestTypes.boss)
            return new BossQuest(players, GM);
        if (currQuestType == QuestTypes.jump)
            return new JumpQuest(players, GM);
        if (currQuestType == QuestTypes.race)
            return new RacingQuest(players, GM);

        Debug.Log("need to implement and assign quest type: " + currQuestType);
        return new CollectQuest(players, GM);

    }



}
