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
    public class GamePlayerTests
    {
        [TestMethod()]
        public void CheckWinTest()
        {
            GamePlayer player = new GamePlayer();
            player.InitPlayer(1, 1, false, true);



            //1
            {
                player.ListeningTiles = new DaQingMahJong_Server.GameLogic.ListeningTilesData
                {
                    eatMahJongTypeList = new List<List<MahJongType>> {
                        new List<MahJongType> { MahJongType.Thousand1, MahJongType.Thousand2, MahJongType.Thousand3, },
                    },
                    listenItemList = new List<ListenItem>
                    {
                        new ListenItem
                        {
                            listenType = ListenType.SandwichWay,
                            wildMahJongTypeList = new List<List<MahJongType>>()
                            {
                                new List<MahJongType>{MahJongType.Circle9,MahJongType.Circle9,MahJongType.Circle9, },
                                new List<MahJongType>{MahJongType.Stick7,MahJongType.Stick7, MahJongType.Stick7, },
                            },
                            listenTiles = new List<MahJongType>{MahJongType.Thousand1, MahJongType.Thousand3, },
                            otherTiles = new List<MahJongType>{ MahJongType.Thousand5, MahJongType.Thousand5, },
                            winTiles = new List<MahJongType>{ MahJongType.Thousand2},
                        },
                        new ListenItem
                        {
                            listenType = ListenType.StrongWind,
                            wildMahJongTypeList = new List<List<MahJongType>>()
                            {
                                new List<MahJongType>{MahJongType.Circle9,MahJongType.Circle9,MahJongType.Circle9, },
                                new List<MahJongType>{MahJongType.Stick7,MahJongType.Stick7, MahJongType.Stick7, },
                            },
                            listenTiles = new List<MahJongType>{MahJongType.Circle9, MahJongType.Circle9,MahJongType.Circle9, },
                            otherTiles = new List<MahJongType>(),
                            winTiles = new List<MahJongType>{ MahJongType.Circle9},
                        },
                        new ListenItem
                        {
                            listenType = ListenType.StrongWind,
                            wildMahJongTypeList = new List<List<MahJongType>>()
                            {
                                new List<MahJongType>{MahJongType.Circle9,MahJongType.Circle9,MahJongType.Circle9, },
                                new List<MahJongType>{MahJongType.Stick7,MahJongType.Stick7, MahJongType.Stick7, },
                            },
                            listenTiles = new List<MahJongType>{MahJongType.Stick7,MahJongType.Stick7, MahJongType.Stick7, },
                            otherTiles = new List<MahJongType>(),
                            winTiles = new List<MahJongType>{ MahJongType.Stick7},
                        }
                    },
                };

                WinOperationData winOperationData = new WinOperationData();
                winOperationData.mahJongType = MahJongType.Thousand2;

                bool win = player.CheckWin(ref winOperationData, true);


                Assert.IsTrue(win);

            }
            


        }
    }
}