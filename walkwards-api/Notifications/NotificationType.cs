namespace walkwards_api.Notifications;

public enum NotificationType
{
    send_friend_requst, //active
    accept_friend_request, //
    reject_friend_request, //
    remove_friend,
    send_challenge_request,          //active
    accept_challenge_request,   //
    reject_challenge_request,   //
    challenge_end,  //
    give_up_challenge,  //
    competition_end, //
    auction_start,
    auction_end,
    auction_outbid, //active
    send_daily_raport,
    invite_to_guild,
    join_request_guild, //active TODO add to active notifications
    accepted_invite_to_guild,
    reject_invite_to_guild,
    remove_from_guild
}