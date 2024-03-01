using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Personality.Lovin.GUI;

public static class MindCardUtility
{
    private static readonly string LikesText = "PawnLikes".Translate();
    private static readonly string DislikesText = "PawnDislikes".Translate();

    private static float CardHeight => 15f;

    //private static readonly float LabelWidth = Math.Max(Text.CalcSize(LikesText).x, Text.CalcSize(DislikesText).x);

    public static void DrawMindCardRomanceStuff(Rect rect, Pawn pawn)
    {
        RomanceComp comp = pawn.GetComp<RomanceComp>();
        AttractionTracker attractionTracker = comp?.AttractionTracker;

        float yStart = rect.y;

        List<Preference> likes = (from pref in attractionTracker.AllPrefs
                                  where pref.Value >= 0.25f
                                  select pref).ToList();

        List<Preference> dislikes = (from pref in attractionTracker.AllPrefs
                                     where pref.Value <= -0.25f
                                     select pref).ToList();

        Rect likesRect = new(rect.x, rect.y, rect.width, CalcSizeOfSection(likes));

        float yEnd = DrawPreferenceSection(likes, likesRect, LikesText);

        Rect dislikesRect = new(rect.x, yEnd, rect.width, CalcSizeOfSection(dislikes));
        DrawPreferenceSection(dislikes, dislikesRect, DislikesText);
    }

    private static float DrawPreferenceSection(List<Preference> prefs, Rect rect, string label)
    {
        float yStart = rect.y;
        if (prefs.Count > 0)
        {
            Text.Font = GameFont.Small;
            float LabelWidth = Math.Max(Text.CalcSize(LikesText).x, Text.CalcSize(DislikesText).x);
            Rect labelRect = new(rect.x, rect.y, LabelWidth, Text.CalcHeight(label, LabelWidth));
            Widgets.Label(labelRect, label);
            foreach (Preference pref in prefs)
            {
                Text.Font = GameFont.Tiny;
                Vector2 cardSize = Text.CalcSize(pref.Label);
                Rect cardRect = new(rect.x + LabelWidth + 5f, yStart, cardSize.x, cardSize.y);
                Widgets.Label(cardRect, pref.Label);
                yStart += cardSize.y;
            }
        }

        return yStart;
    }

    private static float CalcSizeOfSection(List<Preference> itemsToRender)
    {
        if (itemsToRender.Count == 0) return 0f;

        return itemsToRender.Count * CardHeight;
    }
}