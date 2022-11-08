using System;
using Newtonsoft.Json;


namespace MakingFuss.Data
{
    public class SlackUserProfileResponse
    {
        [JsonProperty("ok")]
        public bool Ok { get; set; }

        [JsonProperty("user")]
        public SlackUserInfo User { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }

    public class SlackUserInfo
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("team_id")]
        public string TeamId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("deleted")]
        public bool Deleted { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("real_name")]
        public string RealName { get; set; }

        [JsonProperty("tz")]
        public string Tz { get; set; }

        [JsonProperty("tz_label")]
        public string TzLabel { get; set; }

        [JsonProperty("tz_offset")]
        public long TzOffset { get; set; }

        [JsonProperty("profile")]
        public SlackUserProfile Profile { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("is_owner")]
        public bool IsOwner { get; set; }

        [JsonProperty("is_primary_owner")]
        public bool IsPrimaryOwner { get; set; }

        [JsonProperty("is_restricted")]
        public bool IsRestricted { get; set; }

        [JsonProperty("is_ultra_restricted")]
        public bool IsUltraRestricted { get; set; }

        [JsonProperty("is_bot")]
        public bool IsBot { get; set; }

        [JsonProperty("is_app_user")]
        public bool IsAppUser { get; set; }

        [JsonProperty("updated")]
        public long Updated { get; set; }
    }

    public class SlackUserProfile
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("skype")]
        public string Skype { get; set; }

        [JsonProperty("real_name")]
        public string RealName { get; set; }

        [JsonProperty("real_name_normalized")]
        public string RealNameNormalized { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("display_name_normalized")]
        public string DisplayNameNormalized { get; set; }

        [JsonProperty("status_text")]
        public string StatusText { get; set; }

        [JsonProperty("status_emoji")]
        public string StatusEmoji { get; set; }

        [JsonProperty("status_expiration")]
        public long StatusExpiration { get; set; }

        [JsonProperty("avatar_hash")]
        public string AvatarHash { get; set; }

        [JsonProperty("image_original")]
        public Uri ImageOriginal { get; set; }

        [JsonProperty("is_custom_image")]
        public bool IsCustomImage { get; set; }

        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("image_24")]
        public Uri Image24 { get; set; }

        [JsonProperty("image_32")]
        public Uri Image32 { get; set; }

        [JsonProperty("image_48")]
        public Uri Image48 { get; set; }

        [JsonProperty("image_72")]
        public Uri Image72 { get; set; }

        [JsonProperty("image_192")]
        public Uri Image192 { get; set; }

        [JsonProperty("image_512")]
        public Uri Image512 { get; set; }

        [JsonProperty("image_1024")]
        public Uri Image1024 { get; set; }

        [JsonProperty("status_text_canonical")]
        public string StatusTextCanonical { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }
    }

}