﻿using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace RimWorldDaysMatter
{
    public class DialogList : Window
    {
        private const int ROW_HEIGHT = 32;
        private const int COL_MARGIN = 5;
        private const int SCROLLBAR_WIDTH = 16;
        private const int LBL_DATE_WIDTH = 50;
        private const int BTN_SEASON_WIDTH = 100;

        private Vector2 _scrollPosition = Vector2.zero;

        private readonly List<QuadrumDayPair> _list = new List<QuadrumDayPair>();

        public DialogList(ListType type)
        {
            forcePause = true;
            doCloseX = true;
            // closeOnEscapeKey = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;

            int gameStartTick = Find.TickManager.gameStartAbsTick;
            var colonists = Find.CurrentMap.mapPawns.PawnsInFaction(Faction.OfPlayer);
            switch (type)
            {
                case ListType.Birthdays:
                    foreach (var colonist in colonists)
                    {
                        if (colonist.Dead || colonist.NonHumanlikeOrWildMan())
                            continue;

                        long birthdayTick = colonist.ageTracker.BirthAbsTicks;
                        int birthDate = GenDate.DayOfQuadrum(birthdayTick, 0); // zero based
                        Quadrum birthQuadrum = GenDate.Quadrum(birthdayTick, 0);
                        _list.Add(new QuadrumDayPair("DM.Letter.BirthdayParty".Translate(colonist.Name.ToStringShort), birthQuadrum, birthDate + 1));
                    }
                    break;
                case ListType.Relationships:
                    Dictionary<Pawn, DirectPawnRelation> loverRelations = new Dictionary<Pawn, DirectPawnRelation>();
                    foreach (var colonist in colonists)
                    {
                        if (colonist.Dead || colonist.NonHumanlikeOrWildMan())
                            continue;

                        List<DirectPawnRelation> relations = colonist.relations.DirectRelations.FindAll(x => x.def == PawnRelationDefOf.Lover);
                        foreach (DirectPawnRelation relation in relations)
                        {
                            if (loverRelations.ContainsKey(colonist) || loverRelations.ContainsKey(relation.otherPawn))
                                continue;
                            loverRelations.Add(colonist, relation);
                            int startTick = relation.startTicks + gameStartTick;
                            int startDay = GenDate.DayOfQuadrum(startTick, 0);
                            Quadrum startQuadrum = GenDate.Quadrum(startTick, 0);
                            _list.Add(new QuadrumDayPair("DM.Letter.RelationshipAnniversaryParty".Translate(colonist.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), startQuadrum, startDay + 1));
                        }
                    }
                    break;
                case ListType.Marriages:
                    Dictionary<Pawn, DirectPawnRelation> marriageRelations = new Dictionary<Pawn, DirectPawnRelation>();
                    foreach (var colonist in colonists)
                    {
                        if (colonist.Dead || colonist.NonHumanlikeOrWildMan())
                            continue;

                        List<DirectPawnRelation> relations = colonist.relations.DirectRelations.FindAll(x => x.def == PawnRelationDefOf.Spouse);
                        foreach (DirectPawnRelation relation in relations)
                        {
                            if (marriageRelations.ContainsKey(colonist) || marriageRelations.ContainsKey(relation.otherPawn))
                                continue;
                            marriageRelations.Add(colonist, relation);
                            int startTick = relation.startTicks + gameStartTick;
                            int startDay = GenDate.DayOfQuadrum(startTick, 0);
                            Quadrum startQuadrum = GenDate.Quadrum(startTick, 0);
                            _list.Add(new QuadrumDayPair("DM.Letter.MarriageAnniversaryParty".Translate(colonist.Name.ToStringShort, relation.otherPawn.Name.ToStringShort), startQuadrum, startDay + 1));
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override void DoWindowContents(Rect rect)
        {
            Text.Font = GameFont.Small;

            Rect rectHeader = new Rect(0f, 0f, rect.width - SCROLLBAR_WIDTH, ROW_HEIGHT);
            DrawHeader(rectHeader);


            float scrollRectOffsetTop = ROW_HEIGHT;
            Rect scrollRect = new Rect(0f, scrollRectOffsetTop, rect.width, rect.height - scrollRectOffsetTop);
            Rect viewRect = new Rect(0f, 0f, scrollRect.width - SCROLLBAR_WIDTH, ROW_HEIGHT * _list.Count);

            Widgets.BeginScrollView(scrollRect, ref _scrollPosition, viewRect);
            Vector2 cur = Vector2.zero;

            for (int index = 0; index < _list.Count; index++)
            {
                if (_list.Count <= index)
                    break;

                var row = new Rect(0f, cur.y, viewRect.width, ROW_HEIGHT);

                Widgets.DrawHighlightIfMouseover(row);
                GUI.color = new Color(1f, 1f, 1f, 0.2f);
                Widgets.DrawLineHorizontal(0f, cur.y, viewRect.width);
                GUI.color = Color.white;

                DrawRow(row, _list[index]);

                cur.y += ROW_HEIGHT;
            }
            Widgets.EndScrollView();
        }

        private void DrawHeader(Rect rect)
        {
            GUI.BeginGroup(rect);

            Rect rectName = rect.AtZero();
            rectName.width -= COL_MARGIN * 2 + BTN_SEASON_WIDTH + LBL_DATE_WIDTH;
            rectName.xMin += COL_MARGIN;

            Rect rectSeason = new Rect(rectName.xMax + COL_MARGIN, 0f, BTN_SEASON_WIDTH, rect.height);
            Rect rectDate = new Rect(rectSeason.xMax + COL_MARGIN, 0f, LBL_DATE_WIDTH, rect.height);

            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(rectName, "DM.Tab.TableHeader.Name".Translate());
            Widgets.Label(rectSeason, "DM.Tab.TableHeader.Quadrum".Translate());
            Widgets.Label(rectDate, "DM.Tab.TableHeader.Day".Translate());
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
        }

        private void DrawRow(Rect rect, QuadrumDayPair pair)
        {
            GUI.BeginGroup(rect);

            Rect rectName = rect.AtZero();
            rectName.width -= COL_MARGIN * 2 + BTN_SEASON_WIDTH + LBL_DATE_WIDTH;
            rectName.xMin += COL_MARGIN;

            Rect rectSeason = new Rect(rectName.xMax + COL_MARGIN, 0f, BTN_SEASON_WIDTH, rect.height);
            Rect rectDate = new Rect(rectSeason.xMax + COL_MARGIN, 0f, LBL_DATE_WIDTH, rect.height);

            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(rectName, pair.Name);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rectSeason, pair.Quadrum.Label());
            Widgets.Label(rectDate, pair.Day.ToString());
            Text.Anchor = TextAnchor.UpperLeft;

            GUI.EndGroup();
        }

        public enum ListType
        {
            Birthdays,
            Relationships,
            Marriages
        }

        private class QuadrumDayPair
        {
            public string Name { get; }
            public Quadrum Quadrum { get; }
            public int Day { get; }

            public QuadrumDayPair(string name, Quadrum quadrum, int day)
            {
                Name = name;
                Quadrum = quadrum;
                Day = day;
            }
        }
    }
}