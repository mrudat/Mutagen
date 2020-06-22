using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mutagen.Bethesda.Oblivion
{
    public enum Function
    {
        CanHaveFlames = 153,
        CanPayCrimeGold = 127,
        GetActorValue = 14,
        GetAlarmed = 61,
        GetAmountSoldStolen = 190,
        GetAngle = 8,
        GetArmorRating = 81,
        GetArmorRatingUpperBody = 274,
        GetAttacked = 63,
        GetBarterGold = 264,
        GetBaseActorValue = 277,
        GetClassDefaultMatch = 229,
        GetClothingValue = 41,
        GetCrime = 122,
        GetCrimeGold = 116,
        GetCurrentAIPackage = 110,
        GetCurrentAIProcedure = 143,
        GetCurrentTime = 18,
        GetCurrentWeatherPercent = 148,
        GetDayOfWeek = 170,
        GetDead = 46,
        GetDeadCount = 84,
        GetDestroyed = 203,
        GetDetected = 45,
        GetDetectionLevel = 180,
        GetDisabled = 35,
        GetDisease = 39,
        GetDisposition = 76,
        GetDistance = 1,
        GetDoorDefaultOpen = 215,
        GetEquipped = 182,
        GetFactionRank = 73,
        GetFactionRankDifference = 60,
        GetFatiguePercentage = 128,
        GetFriendHit = 288,
        GetFurnitureMarkerID = 160,
        GetGlobalValue = 74,
        GetGold = 48,
        GetHeadingAngle = 99,
        GetIdleDoneOnce = 318,
        GetIgnoreFriendlyHits = 338,
        GetInCell = 67,
        GetInCellParam = 230,
        GetInFaction = 71,
        GetInSameCell = 32,
        GetInvestmentGold = 305,
        GetInWorldspace = 310,
        GetIsAlerted = 91,
        GetIsClass = 68,
        GetIsClassDefault = 228,
        GetIsCreature = 64,
        GetIsCurrentPackage = 161,
        GetIsCurrentWeather = 149,
        GetIsGhost = 237,
        GetIsID = 72,
        GetIsPlayableRace = 254,
        GetIsPlayerBirthsign = 224,
        GetIsRace = 69,
        GetIsReference = 136,
        GetIsSex = 70,
        GetIsUsedItem = 246,
        GetIsUsedItemType = 247,
        GetItemCount = 47,
        GetKnockedState = 107,
        GetLevel = 80,
        GetLineOfSight = 27,
        GetLocked = 5,
        GetLockLevel = 65,
        GetNoRumors = 320,
        GetOffersServicesNow = 255,
        GetOpenState = 157,
        GetPCExpelled = 193,
        GetPCFactionAttack = 199,
        GetPCFactionMurder = 195,
        GetPCFactionSteal = 197,
        GetPCFactionSubmitAuthority = 201,
        GetPCFame = 249,
        GetPCInFaction = 132,
        GetPCInfamy = 251,
        GetPCIsClass = 129,
        GetPCIsRace = 130,
        GetPCIsSex = 131,
        GetPCMiscStat = 312,
        GetPersuasionNumber = 225,
        GetPlayerControlsDisabled = 98,
        GetPlayerHasLastRiddenHorse = 362,
        GetPlayerInSEWorld = 365,
        GetPos = 6,
        GetQuestRunning = 56,
        GetQuestVariable = 79,
        GetRandomPercent = 77,
        GetRestrained = 244,
        GetScale = 24,
        GetScriptVariable = 53,
        GetSecondsPassed = 12,
        GetShouldAttack = 66,
        GetSitting = 159,
        GetSleeping = 49,
        GetStage = 58,
        GetStageDone = 59,
        GetStartingAngle = 11,
        GetStartingPos = 10,
        GetTalkedToPC = 50,
        GetTalkedToPCParam = 172,
        GetTimeDead = 361,
        GetTotalPersuasionNumber = 315,
        GetTrespassWarningLevel = 144,
        GetUnconscious = 242,
        GetUsedItemActivate = 259,
        GetUsedItemLevel = 258,
        GetVampire = 40,
        GetWalkSpeed = 142,
        GetWeaponAnimType = 108,
        GetWeaponSkillType = 109,
        GetWindSpeed = 147,
        HasFlames = 154,
        HasMagicEffect = 214,
        HasVampireFed = 227,
        IsActor = 353,
        IsActorAVictim = 314,
        IsActorEvil = 313,
        IsActorUsingATorch = 306,
        IsCellOwner = 280,
        IsCloudy = 267,
        IsContinuingPackagePCNear = 150,
        IsCurrentFurnitureObj = 163,
        IsCurrentFurnitureRef = 162,
        IsEssential = 354,
        IsFacingUp = 106,
        IsGuard = 125,
        IsHorseStolen = 282,
        IsIdlePlaying = 112,
        IsInCombat = 289,
        IsInDangerousWater = 332,
        IsInInterior = 300,
        IsInMyOwnedCell = 146,
        IsLeftUp = 285,
        IsOwner = 278,
        IsPCAMurderer = 176,
        IsPCSleeping = 175,
        IsPlayerInJail = 171,
        IsPlayerMovingIntoNewSpace = 358,
        IsPlayersLastRiddenHorse = 339,
        IsPleasant = 266,
        IsRaining = 62,
        IsRidingHorse = 327,
        IsRunning = 287,
        IsShieldOut = 103,
        IsSneaking = 286,
        IsSnowing = 75,
        IsSpellTarget = 223,
        IsSwimming = 185,
        IsTalking = 141,
        IsTimePassing = 265,
        IsTorchOut = 102,
        IsTrespassing = 145,
        IsTurnArrest = 329,
        IsWaiting = 111,
        IsWeaponOut = 101,
        IsXBox = 309,
        IsYielding = 104,
        MenuMode = 36,
        SameFaction = 42,
        SameFactionAsPC = 133,
        SameRace = 43,
        SameRaceAsPC = 134,
        SameSex = 44,
        SameSexAsPC = 135,
        WhichServiceMenu = 323,

    }
}