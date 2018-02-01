#pragma warning disable 1591

namespace RedditSharp
{
    public enum ModActionType
    {
        BanUser,
        UnBanUser,
        RemoveLink,
        ApproveLink,
        RemoveComment,
        ApproveComment,
        AddModerator,
        InviteModerator,
        UnInviteModerator,
        AcceptModeratorInvite,
        RemoveModerator,
        AddContributor,
        RemoveContributor,
        EditSettings,
        EditFlair,
        Distinguish,
        MarkNSFW,
        WikiBanned,
        WikiContributor,
        WikiUnBanned,
        WikiPageListed,
        RemoveWikiContributor,
        WikiRevise,
        WikiPermlevel,
        IgnoreReports,
        UnIgnoreReports,
        SetPermissions,
        SetSuggestedsort,
        Sticky,
        UnSticky,
        SetContestMode,
        UnSetContestMode,
        @Lock,
        Unlock,
        MuteUser,
        UnMuteUser,
        SpamComment,
        SpamLink,
        CreateRule,
        EditRule,
        DeleteRule,
        Spoiler,
        UnSpoiler,
        Modmail_Enrollment
    }
}
#pragma warning restore 1591