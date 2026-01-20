using CodeStage.AntiCheat.Storage;
using System;
using UnityEngine;

public class SaveGame
{

    /// ///////////////////
    public static int GetNumOfItem(ItemType itemType)
    {
        return ObscuredPrefs.Get($"item{(int)itemType}", 0);
    }

    public static int AddItem(ItemType itemType, int addedQuantity)
    {
        int curQuantity = GetNumOfItem(itemType);
        ObscuredPrefs.Set($"item{(int)itemType}", curQuantity + addedQuantity);
        return curQuantity + addedQuantity;
    }
    public static void SetNumOfItem(ItemType itemType, int quantity)
    {
        ObscuredPrefs.Set($"item{(int)itemType}", quantity);
    }
    public static bool GetCacheFlag(CacheFlag flagEnum)
    {
        int value = (int)flagEnum;
        int flagId = value / 30;
        return IsBitOn(PlayerPrefs.GetInt($"FlagC{flagId}", 0), value % 30);
    }
    public static void SetCacheFlag(CacheFlag flagEnum, bool isOn)
    {
        int value = (int)flagEnum;
        int flagId = value / 30;
        int state = PlayerPrefs.GetInt($"FlagC{flagId}", 0);
        PlayerPrefs.SetInt($"FlagC{flagId}", SetBit(state, (int)flagEnum % 30, isOn));
    }

    public static int GetCacheNumber(CacheNumber numEnum, int defaultValue = -1)
    {
        return PlayerPrefs.GetInt(numEnum.ToString(), defaultValue);
    }
    public static void SetCacheNumber(CacheNumber numEnum, int value)
    {
        PlayerPrefs.SetInt(numEnum.ToString(), value);
    }
    public static bool GetBool(string keyString, bool defaultValue = false)
    {
        return ObscuredPrefs.Get(keyString, defaultValue);
    }
    public static void SetBool(string keyString, bool value)
    {
        ObscuredPrefs.Set(keyString, value);
    }

    public static bool GetFlag(SaveFlag flagEnum)
    {
        int value = (int)flagEnum;
        int flagId = value / 30;
        return IsBitOn(ObscuredPrefs.Get($"Flag{flagId}", 0), value % 30);
    }
    public static void SetFlag(SaveFlag flagEnum, bool isOn)
    {
        int value = (int)flagEnum;
        int flagId = value / 30;
        int state = ObscuredPrefs.Get($"Flag{flagId}", 0);
        ObscuredPrefs.Set($"Flag{flagId}", SetBit(state, (int)flagEnum % 30, isOn));
    }


    public static int GetNumber(SaveNumber numEnum, int defaultValue = -1)
    {
        return ObscuredPrefs.Get(numEnum.ToString(), defaultValue);
    }

    public static void SetNumber(SaveNumber numEnum, int value)
    {
        ObscuredPrefs.Set(numEnum.ToString(), value);
    }
    public static float GetFloatNumber(SaveNumber numEnum, float defaultValue = -1)
    {
        return ObscuredPrefs.Get(numEnum.ToString(), defaultValue);
    }

    public static void SetFloatNumber(SaveNumber numEnum, float value)
    {
        ObscuredPrefs.Set(numEnum.ToString(), value);
    }

    public static int GetNumber(string key, int defaultValue = 0)
    {
        return ObscuredPrefs.Get(key, defaultValue);
    }

    public static void SetNumber(string key, int value)
    {
        ObscuredPrefs.Set(key, value);
    }

    ////////////////////////////////////////////////////

    public static long GetUnixTimestamp()
    {
        return (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
    public static int GetDayTimestamp()
    {
        return (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalDays;
    }
    public static DateTime GetDateTime()
    {
        return DateTime.UtcNow;
    }

    public static int GetWeekTimestamp()
    {
        return (GetDayTimestamp() - 2) / 7;
    }
    public static DateTime GetStartDayOfWeek(int weekIndex)
    {
        return (new DateTime(1970, 1, 3)).AddDays(weekIndex * 7);
    }

    public static bool IsBitOn(int num, int bitIndex)
    {
        return (num & (1 << bitIndex)) > 0;
    }
    public static int SetBit(int num, int bitIndex, bool isOn)
    {
        if (isOn) return num | (1 << bitIndex);
        return num & (~(1 << bitIndex));
    }
}


public enum SaveFlag
{
    noAds,
    boughtPremium,
    playedDaily,
    claimedNoAdsDaily,
    clearItemUnlocked,
    clearConveyorUnlocked,
    unlockedInstructionHiddenCookie,
    firstLaunch,
    unlockedInstructionLockHole,
    unlockedInstructionHiddenHole,
}
public enum SaveNumber
{
    LastCheckInDay,
    LastCheckInWeek,

    firstDayOfWeek,
    curLocalMonth,
    dailyCompleteState,
    playedDaily,
    firstWeekEver,
    firstLocalMonthEver,
    CurLevelId,
    PrevGiftProgress,

    SfxOff,
    MusicOff,
    UnlockedFeatureCount,
    freeHammerBoosterUses
}

public enum CacheFlag
{
    TurnOnMusic,
    TurnOnHaptics,
    TurnOnSfx,
    BoughtAny,
}
public enum CacheNumber
{
    Language,
}



public enum Language
{
    English,
    French,
}

public enum ItemType
{
    ClearItem,
    ClearConveyor,
    Hint,
    Coin,
}