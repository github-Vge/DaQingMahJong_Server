using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaQingMahJong_Server.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DaQingMahJong_Server.GameLogic.Tests
{
    [TestClass()]
    public class ListenManagerTests
    {
        [TestMethod()]
        public void CheckListeningTest()
        {
            //新建输入数据
            List<MahJongType> mahJongTypeList;
            List<List<MahJongType>> eatMahJongTypeList;
            //新建输出数据
            ListeningTilesData listeningTilesData;
            List<ListenItem> listenItemList;
            bool canListen = true;
            bool equal = true;

            //1
            {
                //测试输入用例
                eatMahJongTypeList = new List<List<MahJongType>>
            {
                new List<MahJongType>{MahJongType.Circle1, MahJongType.Circle1, MahJongType.Circle1, },
                new List<MahJongType>{MahJongType.Thousand7, MahJongType.Thousand7, MahJongType.Thousand7, },
                new List<MahJongType> {MahJongType.Stick7, MahJongType.Stick8, MahJongType.Stick9, },
            };
                mahJongTypeList = new List<MahJongType> { MahJongType.Circle4, MahJongType.Circle4, MahJongType.Circle6, MahJongType.Circle7, };

                canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out listeningTilesData);

                Assert.IsTrue(canListen);

                equal = true;
                //测试输出用例
                listenItemList = new List<ListenItem>
                {
                    new ListenItem
                    {
                        listenType = ListenType.SideWay,
                        wildMahJongTypeList = new List<List<MahJongType>>(),
                        listenTiles = new List<MahJongType>{MahJongType.Circle6, MahJongType.Circle7, },
                        otherTiles = new List<MahJongType>{ MahJongType.Circle4, MahJongType.Circle4, },
                        winTiles = new List<MahJongType>{ MahJongType.Circle5, MahJongType.Circle8, },
                    },
                    new ListenItem
                    {
                        listenType = ListenType.StrongWind,
                        wildMahJongTypeList = new List<List<MahJongType>>(),
                        listenTiles = new List<MahJongType>{MahJongType.Circle1, MahJongType.Circle1,MahJongType.Circle1, },
                        otherTiles = new List<MahJongType>(),
                        winTiles = new List<MahJongType>{ MahJongType.Circle1, },
                    },
                    new ListenItem
                    {
                        listenType = ListenType.StrongWind,
                        wildMahJongTypeList = new List<List<MahJongType>>(),
                        listenTiles = new List<MahJongType>{MahJongType.Thousand7, MahJongType.Thousand7, MahJongType.Thousand7, },
                        otherTiles = new List<MahJongType>(),
                        winTiles = new List<MahJongType>{ MahJongType.Thousand7, },
                    },
                };
                //先排序
                listenItemList = listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                listeningTilesData.listenItemList = listeningTilesData.listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                for (int i = 0; i < listeningTilesData.listenItemList.Count; i++)
                {
                    if (listenItemList[i] != listeningTilesData.listenItemList[i])
                    {
                        equal = false;
                        break;
                    }
                }
                Assert.IsTrue(equal);
            }


            //2
            {
                //测试输入用例
                eatMahJongTypeList = new List<List<MahJongType>>
            {
                new List<MahJongType>{MahJongType.Stick5, MahJongType.Stick5, MahJongType.Stick5, },
                new List<MahJongType>{MahJongType.Circle7, MahJongType.Circle8, MahJongType.Circle9, },
                new List<MahJongType> {MahJongType.Circle4, MahJongType.Circle5, MahJongType.Circle6, },
            };
                mahJongTypeList = new List<MahJongType> { MahJongType.RedDragon, MahJongType.RedDragon, MahJongType.Stick8, MahJongType.Stick9, };

                canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out listeningTilesData);

                Assert.IsTrue(canListen);

                equal = true;
                //测试输出用例
                listenItemList = new List<ListenItem>
                {
                    new ListenItem
                    {
                        listenType = ListenType.SandwichWay,
                        wildMahJongTypeList = new List<List<MahJongType>>(),
                        listenTiles = new List<MahJongType>{MahJongType.Stick8, MahJongType.Stick9, },
                        otherTiles = new List<MahJongType>{ MahJongType.RedDragon, MahJongType.RedDragon, },
                        winTiles = new List<MahJongType>{ MahJongType.Stick7},
                    },
                    new ListenItem
                    {
                        listenType = ListenType.StrongWind,
                        wildMahJongTypeList = new List<List<MahJongType>>(),
                        listenTiles = new List<MahJongType>{MahJongType.Stick5, MahJongType.Stick5, MahJongType.Stick5, },
                        otherTiles = new List<MahJongType>(),
                        winTiles = new List<MahJongType>{ MahJongType.Stick5, },
                    },
                };
                //先排序
                listenItemList = listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                listeningTilesData.listenItemList = listeningTilesData.listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                for (int i = 0; i < listeningTilesData.listenItemList.Count; i++)
                {
                    if (listenItemList[i] != listeningTilesData.listenItemList[i])
                    {
                        equal = false;
                        break;
                    }
                }
                Assert.IsTrue(equal);
            }



            //3
            //测试输入用例
            eatMahJongTypeList = new List<List<MahJongType>>
            {
                new List<MahJongType>{MahJongType.Stick7, MahJongType.Stick8, MahJongType.Stick9, },
            };
            mahJongTypeList = new List<MahJongType> { 
                MahJongType.RedDragon, 
                MahJongType.Circle5, MahJongType.Circle6, MahJongType.Circle7, 
                MahJongType.Stick2, MahJongType.Stick3, MahJongType.Stick4, MahJongType.Stick5, MahJongType.Stick6, MahJongType.Stick8,
            };

            canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out _);

            Assert.IsFalse(canListen);



            //4
            {
                //测试输入用例
                eatMahJongTypeList = new List<List<MahJongType>>
            {
                new List<MahJongType>{MahJongType.Thousand3, MahJongType.Thousand4, MahJongType.Thousand5, },
                new List<MahJongType>{MahJongType.Stick1, MahJongType.Stick2, MahJongType.Stick3, },
            };
                mahJongTypeList = new List<MahJongType> {
                MahJongType.Stick3, MahJongType.Stick4, MahJongType.Stick5,
                MahJongType.Circle1, MahJongType.Circle1,MahJongType.Circle6,MahJongType.Circle6,
            };

                canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out listeningTilesData);

                Assert.IsTrue(canListen);

                equal = true;
                //测试输出用例
                listenItemList = new List<ListenItem>
                {
                    new ListenItem
                    {
                        listenType = ListenType.TwoDoubleTile,
                        wildMahJongTypeList = new List<List<MahJongType>>()
                        {
                            new List<MahJongType>{MahJongType.Stick3,MahJongType.Stick4,MahJongType.Stick5,},
                        },
                        listenTiles = new List<MahJongType>{MahJongType.Circle1, MahJongType.Circle1, },
                        otherTiles = new List<MahJongType>{ MahJongType.Circle6, MahJongType.Circle6, },
                        winTiles = new List<MahJongType>{ MahJongType.Circle1,MahJongType.Circle6},
                    }
                };
                //先排序
                listenItemList = listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                listeningTilesData.listenItemList = listeningTilesData.listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                for (int i = 0; i < listeningTilesData.listenItemList.Count; i++)
                {
                    if (listenItemList[i] != listeningTilesData.listenItemList[i])
                    {
                        equal = false;
                        break;
                    }
                }
                Assert.IsTrue(equal);
            }



            //5
            //测试输入用例
            eatMahJongTypeList = new List<List<MahJongType>>
            {
                new List<MahJongType>{MahJongType.Thousand6, MahJongType.Thousand6, MahJongType.Thousand6, },
                new List<MahJongType>{MahJongType.RedDragon, MahJongType.RedDragon, MahJongType.RedDragon, },
                new List<MahJongType>{MahJongType.Stick4, MahJongType.Stick5, MahJongType.Stick6, },
            };
            mahJongTypeList = new List<MahJongType> {
                MahJongType.Circle2, MahJongType.Circle5,  MahJongType.Stick3, MahJongType.Stick5,
            };

            canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out _);

            Assert.IsFalse(canListen);



            //6 两个对
            {
                //测试输入用例
                eatMahJongTypeList = new List<List<MahJongType>>
                {
                    new List<MahJongType>{MahJongType.Circle7, MahJongType.Circle8, MahJongType.Circle6, },
                    new List<MahJongType>{MahJongType.Thousand1, MahJongType.Thousand2, MahJongType.Thousand3, },
                };
                mahJongTypeList = new List<MahJongType> {
                    MahJongType.Thousand3,MahJongType.Thousand4,MahJongType.Thousand5,MahJongType.Circle2,
                    MahJongType.RedDragon,MahJongType.Circle2, MahJongType.RedDragon,
                };

                canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out listeningTilesData);

                Assert.IsTrue(canListen);

                equal = true;
                //测试输出用例
                listenItemList = new List<ListenItem>
                {
                    new ListenItem
                    {
                        listenType = ListenType.TwoDoubleTile,
                        wildMahJongTypeList = new List<List<MahJongType>>()
                        {
                            new List<MahJongType>{MahJongType.Thousand3, MahJongType.Thousand4, MahJongType.Thousand5, },
                        },
                        listenTiles = new List<MahJongType>{MahJongType.Circle2, MahJongType.Circle2, },
                        otherTiles = new List<MahJongType>{ MahJongType.RedDragon, MahJongType.RedDragon, },
                        winTiles = new List<MahJongType>{ MahJongType.Circle2, MahJongType.RedDragon },
                    }
                };
                //先排序
                listenItemList = listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                listeningTilesData.listenItemList = listeningTilesData.listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                for (int i = 0; i < listeningTilesData.listenItemList.Count; i++)
                {
                    if (listenItemList[i] != listeningTilesData.listenItemList[i])
                    {
                        equal = false;
                        break;
                    }
                }
                Assert.IsTrue(equal);
            }



            //7 有杠
            {
                //测试输入用例
                eatMahJongTypeList = new List<List<MahJongType>>
                {
                    new List<MahJongType>{MahJongType.Circle6, MahJongType.Circle6, MahJongType.Circle6, MahJongType.Circle6,  },
                };
                mahJongTypeList = new List<MahJongType> {
                    MahJongType.Thousand1,MahJongType.Thousand2,MahJongType.Thousand3,
                    MahJongType.Thousand7,MahJongType.Thousand8, MahJongType.Thousand9,
                    MahJongType.Circle1, MahJongType.Circle3,
                    MahJongType.Stick2, MahJongType.Stick2,
                };

                canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out listeningTilesData);

                Assert.IsTrue(canListen);

                equal = true;
                //测试输出用例
                listenItemList = new List<ListenItem>
                {
                    new ListenItem
                    {
                        listenType = ListenType.SandwichWay,
                        wildMahJongTypeList = new List<List<MahJongType>>()
                        {
                            new List<MahJongType>{MahJongType.Thousand1, MahJongType.Thousand2, MahJongType.Thousand3, },
                            new List<MahJongType>{MahJongType.Thousand7, MahJongType.Thousand8, MahJongType.Thousand9, },
                        },
                        listenTiles = new List<MahJongType>{MahJongType.Circle1, MahJongType.Circle3, },
                        otherTiles = new List<MahJongType>{ MahJongType.Stick2, MahJongType.Stick2, },
                        winTiles = new List<MahJongType>{ MahJongType.Circle2},
                    }
                };
                //先排序
                listenItemList = listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                listeningTilesData.listenItemList = listeningTilesData.listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                for (int i = 0; i < listeningTilesData.listenItemList.Count; i++)
                {
                    if (listenItemList[i] != listeningTilesData.listenItemList[i])
                    {
                        equal = false;
                        break;
                    }
                }
                Assert.IsTrue(equal);
            }


            //8 夹
            {
                //测试输入用例
                eatMahJongTypeList = new List<List<MahJongType>>
                {
                    new List<MahJongType>{MahJongType.Thousand1, MahJongType.Thousand2, MahJongType.Thousand3, },
                };
                mahJongTypeList = new List<MahJongType> {
                    MahJongType.Circle9,MahJongType.Circle9,MahJongType.Circle9,
                    MahJongType.Stick7,MahJongType.Stick7, MahJongType.Stick7,
                    MahJongType.Thousand1, MahJongType.Thousand3,
                    MahJongType.Thousand5, MahJongType.Thousand5,
                };

                canListen = ListenManager.CheckListening(mahJongTypeList, eatMahJongTypeList, out listeningTilesData);

                Assert.IsTrue(canListen);

                equal = true;
                //测试输出用例
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
                };
                //先排序
                listenItemList = listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                listeningTilesData.listenItemList = listeningTilesData.listenItemList.OrderBy(p => p.winTiles.Min()).ToList();
                for (int i = 0; i < listeningTilesData.listenItemList.Count; i++)
                {
                    if (listenItemList[i] != listeningTilesData.listenItemList[i])
                    {
                        equal = false;
                        break;
                    }
                }
                Assert.IsTrue(equal);
            }

        }




    }

}