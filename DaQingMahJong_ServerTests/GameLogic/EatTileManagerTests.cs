using DaQingMahJong_Server.GameLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass()]
    public class EatTileManagerTests
    {
        [TestMethod()]
        public void CheckLeftEatAndListeningTest()
        {
            //构建输出数据
            List<MahJongType> fitEatMahJongTypeList;
            List<MahJongType> listeningMagJongTypeList;


            //1
            MahJongTiles mahJongTiles = new MahJongTiles()
            {
                tiles = new List<MahJongType>
                {
                    MahJongType.RedDragon, MahJongType.RedDragon, MahJongType.RedDragon,
                    MahJongType.Thousand1, MahJongType.Thousand1, MahJongType.Thousand1,
                    MahJongType.Circle1,MahJongType.Circle1,MahJongType.Circle3,MahJongType.Circle3,
                    MahJongType.Circle5,MahJongType.Circle5, MahJongType.Circle6,
                },
                eatTiles = new List<List<MahJongType>>(),

            };

            bool listen = ListenManager.CheckEatAndListening(EatTileType.LeftEat, MahJongType.Circle4, mahJongTiles,out fitEatMahJongTypeList,out listeningMagJongTypeList);

            Assert.IsTrue(listen);

        }
    }
}