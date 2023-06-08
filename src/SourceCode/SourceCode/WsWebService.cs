using System;
using System.Collections.Generic;
using CommonInventory.V4;
using KA.Framework;
using SerializableDictionary;
using UnityEngine;

public class WsWebService
{
	public const string mApiToken = "apiToken";

	public const string mApiKeyName = "apiKey";

	private const string mEventXML = "eventXml";

	public const string mContentXML = "contentXML";

	private const string mImageFile = "imageFile";

	private const string mImageType = "ImageType";

	private const string mImageSlot = "ImageSlot";

	private const string mImageSetRequest = "imageSetRequests";

	private const string mBase64StringData = "base64StringData";

	private const string mImageFormatTypeID = "imageFormatTypeID";

	private const string mPage = "page";

	private const string mQuantity = "quantity";

	private const string mModeID = "modeID";

	private const string mShowOldMessages = "showOldMessages";

	private const string mShowDeletedMessages = "showDeletedMessages";

	public const string mUserID = "userId";

	private const string mUserIDs = "userIds";

	public const string mStoreID = "storeId";

	private const string mUserInventoryID = "userInventoryId";

	private const string mCommonInventoryID = "commonInventoryID";

	private const string mNumberOfUses = "numberOfUses";

	private const string mGameID = "gameId";

	public const string mMissionID = "missionId";

	private const string mSceneName = "sceneName";

	private const string mType = "type";

	private const string mName = "name";

	private const string mModuleName = "ModuleName";

	private const string mPoints = "points";

	private const string mStepID = "stepId";

	public const string mItemID = "itemId";

	private const string mGroupName = "groupName";

	private const string mWorldID = "worldId";

	public const string mTaskID = "taskId";

	public const string mAmount = "amount";

	private const string mChestName = "chestName";

	private const string mUserScore = "userScore";

	private const string mAchievementID = "achievementID";

	private const string mSignature = "signature";

	public const string mContainerID = "ContainerId";

	private const string mWorldObjectID = "worldObjectID";

	public const string mPairID = "pairId";

	private const string mPairKey = "pairKey";

	private const string mPairKeys = "pairKeys";

	private const string mAvatarID = "avatarID";

	private const string mFirstNameID = "firstNameID";

	private const string mSecondNameID = "secondNameID";

	private const string mThirdNameID = "thirdNameID";

	private const string mIsMultiplayer = "isMultiplayer";

	private const string mDifficulty = "difficulty";

	private const string mLevel = "gameLevel";

	private const string mXmlData = "xmlDocumentData";

	private const string mWin = "win";

	private const string mLoss = "loss";

	private const string mTicks = "ticks";

	private const string mKey = "key";

	private const string mCount = "count";

	private const string mAscendingOrder = "AscendingOrder";

	private const string mScore = "score";

	private const string mStartDate = "startDate";

	private const string mEndDate = "endDate";

	private const string mAssetName = "assetName";

	private const string mBuddyUserID = "buddyUserID";

	private const string mBuddyUserIDs = "buddyUserIDs";

	private const string mNickname = "nickName";

	private const string mOtherID = "otherId";

	private const string mOtherIDs = "otherIds";

	private const string mFriendCode = "friendCode";

	private const string mUserMessageQueueID = "userMessageQueueID";

	private const string mIsNew = "isNew";

	private const string mIsDeleted = "isDeleted";

	private const string mContentType = "contentType";

	private const string mProductIDName = "productId";

	private const string mNeighborUserID = "neighboruserid";

	private const string mSlot = "slot";

	private const string mBuddyFilter = "buddyFilter";

	private const string mPackageName = "packagename";

	private const string mCommonInventoryRequestXml = "commonInventoryRequestXml";

	private const string mGetCommonInventoryRequestXml = "getCommonInventoryRequestXml";

	private const string mPairXml = "pairxml";

	private const string mItemIDArrayXml = "itemIDArrayXml";

	private const string mUserInventoryCommonIDs = "userInventoryCommonIDs";

	private const string mCreateXml = "createXml";

	private const string mUpdateXml = "updateXml";

	private const string mRemoveXml = "removeXml";

	private const string mInputXml = "inputXml";

	private const string mFlag = "flag";

	public const string mCurrencyType = "currencyType";

	private const string mUserTimedItemID = "userTimedItemID";

	private const string mStreamPostID = "streamPostID";

	private const string mFacebookUserIDs = "facebookUserIDs";

	public const string mPetTypeID = "petTypeID";

	private const string mPetTypeIDs = "petTypeIDs";

	private const string mIsActive = "isActive";

	private const string mActive = "active";

	public const string mRaisedPetID = "raisedPetID";

	public const string mRaisedPetData = "raisedPetData";

	public const string mUnselectOtherPets = "unselectOtherPets";

	private const string mMemberOnly = "memberOnly";

	private const string mNumberOfDays = "numberOfDays";

	private const string mCategoryID = "categoryID";

	private const string mRatedEntityID = "ratedEntityID";

	private const string mRatedUserID = "ratedUserID";

	private const string mEntityID = "entityID";

	private const string mRatingID = "ratingID";

	private const string mRatedValue = "ratedValue";

	private const string mNumRankRecords = "numberOfRecord";

	private const string mTrackCategoryID = "category";

	private const string mTrackID = "trackID";

	private const string mTrackIDArray = "trackIDs";

	private const string mTrackSetRequest = "trackDataSetRequest";

	private const string mUserIDList = "commaSeperatedUserIdList";

	private const string mTypeID = "typeID";

	private const string mAchievementInfoID = "achievementInfoID";

	private const string mRelatedID = "relatedID";

	private const string mActiveOnly = "activeOnly";

	private const string mMessageText = "messageText";

	private const string mStatusID = "statusID";

	private const string mProfileAnswerIDs = "profileAnswerIDs";

	private const string mGender = "gender";

	private const string mMessageID = "messageID";

	private const string mFilter = "filter";

	private const string mParentEmailID = "parentEmailID";

	private const string mReportUserID = "reportUserID";

	private const string mReportReason = "reportReason";

	private const string mRoomID = "roomID";

	private const string mUserName = "username";

	private const string mPassword = "password";

	private const string mParentLoginData = "parentLoginData";

	public const string mChildUserID = "childUserID";

	private const string mParentApiToken = "parentApiToken";

	private const string mLocale = "locale";

	private const string mLocaleRequest = "getLocaleRequest";

	private const string mEmail = "email";

	private const string mFirstName = "firstName";

	private const string mLastName = "lastName";

	private const string mMonth = "month";

	private const string mAppName = "appName";

	private const string mChildName = "childName";

	private const string mCulture = "culture";

	private const string mNewsletterExtraInfoID = "newsletterExtraInfoID";

	private const string mYear = "year";

	private const string mPointTypeID = "pointTypeID";

	private const string mToUser = "toUser";

	private const string mToUsers = "toUsers";

	private const string mFromUser = "fromUser";

	private const string mTargetUser = "targetUser";

	private const string mMessageContent = "content";

	private const string mMessageLevel = "level";

	private const string mMessageRequest = "messagerequest";

	private const string mReplyTo = "replyTo";

	private const string mDisplayAttribute = "displayAttribute";

	private const string mTargetList = "targetList";

	private const string mData = "data";

	private const string mDateRange = "dateRange";

	private const string mPetTypeFilter = "petTypeFilter";

	private const string mActivePetOnly = "activeOnly";

	private const string mUserCount = "count";

	private const string mParentRegistrationData = "parentRegistrationData";

	private const string mChildRegistrationData = "childRegistrationData";

	private const string mGuestUserName = "guestUserName";

	private const string mVersion = "version";

	private const string mBestBuddy = "bestBuddy";

	private const string mGroupID = "groupID";

	private const string mPetIDs = "petIDs";

	private const string mGameLevelID = "gameLevelID";

	private const string mGameDifficultyID = "gameDifficultyID";

	private const string mExpiryDurationDays = "expiryDurationDays";

	private const string mChallengeID = "challengeID";

	private const string mPointsType = "pointsType";

	private const string mMyInventoryItemID = "myInventoryItemID";

	private const string mOtherUserID = "otherUserID";

	private const string mOtherInventoryItemID = "otherInventoryItemID";

	private const string mFirstNameInviter = "firstNameInviter";

	private const string mFriendEmailIds = "friendEmailds";

	private const string mReceiverUserID = "receiverUserID";

	private const string mFilterName = "filterName";

	private const string mPlatformID = "platformID";

	public const string mCompleted = "completed";

	public const string mXmlPayload = "xmlPayload";

	private const string mGuestUserData = "guestLoginData";

	public const string mRequest = "request";

	private const string mActivationReminderRequest = "activationReminderRequest";

	private const string mSetNextItemStateRequest = "setNextItemStateRequest";

	private const string mSetSpeedUpItemRequest = "setSpeedUpItemRequest";

	private const string mGroupType = "groupType";

	private const string mGroupCreateRequest = "groupCreateRequest";

	private const string mGroupEditRequest = "groupEditRequest";

	private const string mGroupJoinRequest = "groupJoinRequest";

	private const string mGroupLeaveRequest = "groupLeaveRequest";

	private const string mGetGroupsRequest = "getGroupsRequest";

	private const string mRemoveMemberRequest = "removeMemberRequest";

	private const string mAuthorizeJoinRequest = "authorizeJoinRequest";

	private const string mNameValidationRequest = "nameValidationRequest";

	private const string mAssignRoleRequest = "assignRoleRequest";

	private const string mGetPendingJoinRequest = "getPendingJoinRequest";

	private const string mInvitePlayerRequest = "invitePlayerRequest";

	private const string mUpdateInviteRequest = "updateInviteRequest";

	private const string mFriendInviteRequest = "friendInviteRequest";

	private const string mFriendInviteProcessRequest = "inviteProcessRequest";

	private const string mGameDataRequest = "gameDataRequest";

	private const string mAchievementTaskSetRequest = "achievementTaskSetRequest";

	private const string mGetStoreRequest = "getStoreRequest";

	private const string mCode = "code";

	private const string mAchievementTaskID = "achievementTaskId";

	private const string mSellItemsRequest = "sellItemsRequest";

	private const string mFuseItemsRequest = "fuseItemsRequest";

	private const string mAchievementTaskIDList = "achievementTaskIDList";

	private const string mItemRequest = "itemRequests";

	private const string mPurchaseItemRequest = "purchaseItemRequest";

	private const string mExchangeItemIDs = "itemids";

	private static string mContentURL = "http://dev.api.jumpstart.com/ContentServer/ContentWebService.asmx/";

	private static string mContentV2URL = "http://dev.api.jumpstart.com/ContentServer/V2/ContentWebService.asmx/";

	private static string mContentV3URL = "http://dev.api.jumpstart.com/ContentServer/V3/ContentWebService.asmx/";

	private static string mContentV4URL = "http://dev.api.jumpstart.com/ContentServer/V4/ContentWebService.asmx/";

	private static string mAuthenticationURL = "http://dev.api.jumpstart.com/Common/AuthenticationWebService.asmx/";

	private static string mAuthenticationV3URL = "http://dev.api.jumpstart.com/Common/V3/AuthenticationWebService.asmx/";

	private static string mMembershipURL = "http://dev.api.jumpstart.com/Common/MembershipWebService.asmx/";

	private static string mConfigurationURL = "http://dev.api.jumpstart.com/Common/ConfigurationWebService.asmx/";

	private static string mAvatarURL = "http://dev.api.jumpstart.com/common/AvatarWebService.asmx/";

	private static string mMessagingURL = "http://dev.api.jumpstart.com/Common/MessagingWebService.asmx/";

	private static string mAchievementURL = "http://dev.api.jumpstart.com/Achievement/AchievementWebService.asmx/";

	private static string mAchievementV2URL = "http://dev.api.jumpstart.com/Achievement/V2/AchievementWebService.asmx/";

	private static string mItemStoreURL = "http://dev.api.jumpstart.com/ItemStoreMission/ItemStoreWebService.asmx/";

	private static string mMissionURL = "http://dev.api.jumpstart.com/ItemStoreMission/MissionWebService.asmx/";

	private static string mSubscriptionURL = "http://dev.api.jumpstart.com/common/SubscriptionWebService.asmx/";

	private static string mFacebookURL = "http://dev.api.fb.jumpstart.com/facebookwebservice.asmx/";

	private static string mRatingURL = "http://dev.api.jumpstart.com/ContentServer/RatingWebService.asmx/";

	private static string mRatingV2URL = "http://dev.api.jumpstart.com/Contentserver/V2/Ratingwebservice.asmx/";

	private static string mScoreURL = "http://dev.api.jumpstart.com/ContentServer/ScoreWebService.asmx/";

	private static string mTrackURL = "http://dev.api.jumpstart.com/ContentServer/ContentWebService.asmx/";

	private static string mLocaleURL = "http://dev.api.jumpstart.com/ItemStoreMission/LocaleService.asmx/";

	private static string mLocaleV2URL = "http://dev.api.jumpstart.com/ItemStoreMission/V2/LocaleWebService.asmx/";

	private static string mProfileURL = "http://dev.api.jumpstart.com/User/ProfileWebService.asmx/";

	private static string mMessageURL = "http://dev.api.jumpstart.com/Messaging/MessageWebService.asmx/";

	private static string mMessageV2URL = "http://dev.api.jumpstart.com/Messaging/v2/MessageWebService.asmx/";

	private static string mMessageV3URL = "http://dev.api.jumpstart.com/Messaging/v3/MessageWebService.asmx/";

	private static string mChatURL = "http://dev.api.jumpstart.com/common/ChatWebService.asmx/";

	private static string mChallengeURL = "http://dev.api.jumpstart.com/ContentServer/ChallengeWebService.asmx/";

	private static string mRegistrationV3URL = "http://dev.api.jumpstart.com/Common/V3/RegistrationWebService.asmx/";

	private static string mRegistrationV4URL = "http://dev.api.jumpstart.com/Common/V4/RegistrationWebService.asmx/";

	private static string mInviteV2URL = "http://dev.api.jumpstart.com/ContentServer/V2/InviteFriendWebService.asmx/";

	private static string mPushNotificationURL = "http://dev.api.jumpstart.com/PushNotification/RegistrationWebService.asmx/";

	private static string mGroupURL = "http://dev.api.jumpstart.com/Groups/GroupWebService.asmx/";

	private static string mGroupV2URL = "http://dev.api.jumpstart.com/Groups/V2/GroupWebService.asmx/";

	private static string mPaymentURL = "http://dev.api.jumpstart.com/ContentServer/PaymentWebService.asmx/";

	private static string mPaymentV2URL = "http://dev.api.jumpstart.com/ContentServer/V2/PaymentWebService.asmx/";

	private static string mPrizeCodeURL = "http://dev.api.jumpstart.com/Common/PrizeCodeWebService.asmx/";

	private static string mPrizeCodeV2URL = "http://dev.api.jumpstart.com/Common/V2/PrizeCodeWebService.asmx/";

	private static string mApiKey = "873E8F68-FCE1-44EB-96A2-2EFD62DF3AF2";

	private static string mSecret = "386C5C75620848e38B523C62850196BF";

	private static string mUserToken = "68F9B33D-7D69-4640-A0DA-8DBDA3911A4D";

	private static long _Ticks;

	private static int mProductID = 1;

	private const string mFunctionAcceptMission = "AcceptMission";

	private const string mFunctionAcceptTimedMission = "AcceptTimedMission";

	private const string mFunctionDeleteAccount = "DeleteAccountNotification";

	private const string mFunctionDelCurrentPet = "DelCurrentPet";

	private const string mFunctionDelImage = "DelImage";

	private const string mFunctionGetAvatarDisplayDataByUserID = "GetAvatarDisplayDataByUserID";

	private const string mFunctionGetAvatarDisplayData = "GetAvatarDisplayData";

	private const string mFunctionGetAvatarByUserID = "GetAvatarByUserID";

	private const string mFunctionGetAvatar = "GetAvatar";

	private const string mFunctionGetDisplayNameByUserID = "GetDisplayNameByUserID";

	private const string mFunctionGetDefaultNameSuggestion = "GetDefaultNameSuggestion";

	private const string mFunctionSetAvatar = "SetAvatar";

	private const string mFunctionGetInventory = "GetInventory";

	private const string mFunctionSetInventory = "SetInventory";

	private const string mFunctionGetCommonInventory = "GetCommonInventory";

	private const string mFunctionGetCommonInventoryByUserID = "GetCommonInventoryByUserID";

	private const string mFunctionSetCommonInventory = "SetCommonInventory";

	private const string mFunctionSetCommonInventoryAttribute = "SetCommonInventoryAttribute";

	private const string mFunctionRerollUserItem = "RerollUserItem";

	private const string mFunctionGetProduct = "GetProduct";

	private const string mFunctionSetProduct = "SetProduct";

	private const string mFunctionGetScene = "GetScene";

	private const string mFunctionSetScene = "SetScene";

	private const string mFunctionGetHouse = "GetHouse";

	private const string mFunctionSetHouse = "SetHouse";

	private const string mFunctionGetLessonMastery = "GetLessonMastery";

	private const string mFunctionSetLessonMastery = "SetLessonMastery";

	private const string mFunctionSetLessonStatus = "SetLessonStatus";

	private const string mFunctionGetImage = "GetImage";

	private const string mFunctionGetImageByUserId = "GetImageByUserId";

	private const string mFunctionGetImageByProductGroup = "GetImageByProductGroup";

	private const string mFunctionSetImage = "SetImage";

	private const string mFunctionSetImages = "SetImages";

	private const string mFunctionGetCurrentPet = "GetCurrentPet";

	private const string mFunctionGetCurrentPetByUserID = "GetCurrentPetByUserID";

	private const string mFunctionSetCurrentPet = "SetCurrentPet";

	private const string mFunctionGetAdoptedPet = "GetAdoptedPet";

	private const string mFunctionSetAdoptedPet = "SetAdoptedPet";

	private const string mFunctionGetConfigurationSetting = "GetConfigurationSetting";

	private const string mFunctionGetConfigurationSettings = "GetConfigurationSettings";

	private const string mFunctionGetContentByTypeByUser = "GetContentByTypeByUser";

	private const string mFunctionGetSubscriptionInfo = "GetSubscriptionInfo";

	private const string mFunctionExtendMembership = "ExtendMembership";

	private const string mFunctionGetChildList = "GetChildList";

	private const string mFunctionGetDetailedChildList = "GetDetailedChildList";

	private const string mFunctionGetUserInfoByToken = "GetUserInfoByApiToken";

	private const string mFunctionGetValidatedUserID = "GetValidatedUserID";

	private const string mFunctionCreateAvatarContestEntry = "CreateAvatarContestEntry";

	private const string mFunctionIsValidToken = "IsValidApiToken";

	private const string mFunctionIsValidApiToken = "IsValidApiToken_V2";

	private const string mFunctionGetRankAttributeData = "GetRankAttributeData";

	private const string mFunctionGetAllRanks = "GetAllRanks";

	private const string mFunctionGetRankByUserID = "GetRankByUserID";

	private const string mFunctionGetRankByUserIDs = "GetRankByUserIDs";

	private const string mFunctionGetAllRewardTypeMultiplier = "GetAllRewardTypeMultiplier";

	private const string mFunctionGetUserAchievementInfo = "GetUserAchievementInfo";

	private const string mFunctionSetUserAchievement = "SetUserAchievement";

	private const string mFunctionGetUserAchievements = "GetUserAchievements";

	private const string mFunctionGetUserAchievementTaskRedeemableRewards = "GetUserAchievementTaskRedeemableRewards";

	private const string mFunctionRedeemUserAchievementTaskReward = "RedeemUserAchievementTaskReward";

	private const string mFunctionGetAchievementsByUserID = "GetAchievementsByUserID";

	private const string mFunctionGetAchievementTaskInfo = "GetAchievementTaskInfo";

	private const string mFunctionGetPetAchievementsByUserID = "GetPetAchievementsByUserID";

	private const string mFunctionGetTopAchievementPointBuddiesByType = "GetTopAchievementPointBuddiesByType";

	private const string mFunctionGetTopAchievementPointUsersByType = "GetTopAchievementPointUsersByType";

	private const string mFunctionSetUserAchievementAndGetReward = "SetUserAchievementAndGetReward";

	private const string mFunctionSetAchievementAndGetReward = "SetAchievementAndGetReward";

	private const string mFunctionSetAchievementByEntityIDs = "SetAchievementByEntityIDs";

	private const string mFunctionGetAchievementRewardsByAchievementTaskID = "GetAchievementRewardsByAchievementTaskID";

	private const string mFunctionApplyPayout = "ApplyPayout";

	private const string mFunctionGetTopAchievementPointUsers = "GetTopAchievementPointUsers";

	private const string mFunctionGetTopAchievementPointBuddies = "GetTopAchievementPointBuddies";

	private const string mFunctionGetUserMessageQueue = "GetUserMessageQueue";

	private const string mFunctionSaveMessage = "SaveMessage";

	private const string mFunctionSendMessage = "SendMessage";

	private const string mFunctionQueueMessage = "QueueMessage";

	private const string mFunctionSendMessageBulk = "SendMessageBulk";

	private const string mFunctionPostNewMessageToBoardWithRMF = "PostNewMessageToBoardWithRMF";

	private const string mFunctionPostNewMessageToBoard = "PostNewMessageToBoard";

	private const string mFunctionPostNewMessageToList = "PostNewMessageToList";

	private const string mFunctionPostNewMessageToListWithRMF = "PostNewMessageToListWithRMF";

	private const string mFunctionPostNewGroupMessageToList = "PostNewGroupMessageToList";

	private const string mFunctionPostNewMessageToGroupListWithRMF = "PostNewMessageToGroupListWithRMF";

	private const string mFunctionPostMessageReply = "PostMessageReply";

	private const string mFunctionPostMessageReplyWithRMF = "PostMessageReplyWithRMF";

	private const string mFunctionPostGroupMessageReply = "PostGroupMessageReply";

	private const string mFunctionPostGroupMessageReplyWithRMF = "PostGroupMessageReplyWithRMF";

	private const string mFunctionGetItemsInStore = "GetItemsInStore";

	private const string mFunctionGetStore = "GetStore";

	private const string mFunctionUseInventory = "UseInventory";

	private const string mFunctionGetPayout = "GetPayout";

	private const string mFunctionGetItem = "GetItem";

	private const string mFunctionGetItems = "GetItems";

	private const string mFunctionGetTreasureChest = "GetTreasureChest";

	private const string mFunctionGetGameCurrency = "GetGameCurrency";

	private const string mFunctionGetGameCurrencyByUserID = "GetGameCurrencyByUserID";

	private const string mFunctionGetUserGameCurrency = "GetUserGameCurrency";

	private const string mFunctionCollectUserBonus = "CollectUserBonus";

	private const string mFunctionSetGameCurrency = "SetGameCurrency";

	private const string mFunctionGetUserMissionState = "GetUserMissionState";

	private const string mFunctionGetUserUpcomingMissionState = "GetUserUpcomingMissionState";

	private const string mFunctionGetUserActiveMissionState = "GetUserActiveMissionState";

	private const string mFunctionGetUserCompletedMissionState = "GetUserCompletedMissionState";

	private const string mFunctionGetUserTimedMissionState = "GetUserTimedMissionState";

	private const string mFunctionGetUserTreasureChest = "GetUserTreasureChest";

	private const string mFunctionSetUserTreasureChest = "SetUserTreasureChest";

	private const string mFunctionPurchaseItems = "PurchaseItems";

	private const string mFunctionSellItems = "SellItems";

	private const string mFunctionSetUserChestFound = "SetUserChestFound";

	private const string mFunctionSetHighScore = "SetHighScore";

	private const string mFunctionGetAnnouncementsByUser = "GetAnnouncementsByUser";

	private const string mFunctionGetDisplayNames = "GetDisplayNames";

	private const string mFunctionSetDisplayName = "SetDisplayName";

	private const string mFunctionGetDisplayNamesByCategory = "GetDisplayNamesByCategoryID";

	private const string mFunctionSetGameData = "SendRawGameData";

	private const string mFunctionGetGameData = "GetGameData";

	private const string mFunctionGetGameDataByUser = "GetGameDataByUser";

	private const string mFunctionGetGameDataByGame = "GetGameDataByGame";

	private const string mFunctionGetGameDataByGameForDayRange = "GetGameDataByGameForDateRange";

	private const string mFunctionGetCumulativePeriodicGameDataByUsers = "GetCumulativePeriodicGameDataByUsers";

	private const string mFunctionGetPeriodicGameDataByGame = "GetPeriodicGameDataByGame";

	private const string mFunctionGetGameDataByGroup = "GetGameDataByGroup";

	private const string mFunctionSetKeyValuePair = "SetKeyValuePair";

	private const string mFunctionSetKeyValuePairByUserID = "SetKeyValuePairByUserID";

	private const string mFunctionGetKeyValuePair = "GetKeyValuePair";

	private const string mFunctionGetKeyValuePairByUserID = "GetKeyValuePairByUserID";

	private const string mFunctionDeleteKeyValuePair = "DelKeyValuePair";

	private const string mFunctionDeleteKeyValuePairByKey = "DelKeyValuePairByKey";

	private const string mFunctionDeleteKeyValuePairByKeys = "DelKeyValuePairByKeys";

	private const string mFunctionGetAssetVersionByAssetName = "GetAssetVersionByAssetName";

	private const string mFunctionGetAllPlatformAssetVersions = "GetAllPlatformAssetVersions";

	private const string mFunctionGetAssetVersions = "GetAssetVersions";

	private const string mFunctionGetAllAssetVersionsByUser = "GetAllAssetVersionsByUser";

	private const string mFunctionGetBuddyList = "GetBuddyList";

	private const string mFunctionGetFriendCode = "GetFriendCode";

	private const string mFunctionAddBuddy = "AddBuddy";

	private const string mFunctionAddBuddyByFriendCode = "AddBuddyByFriendCode";

	private const string mFunctionAddOneWayBuddy = "AddOneWayBuddy";

	private const string mFunctionApproveBuddy = "ApproveBuddy";

	private const string mFunctionBlockBuddy = "BlockBuddy";

	private const string mFunctionGetBuddyLocation = "GetBuddyLocation";

	private const string mFunctionInviteBuddy = "InviteBuddy";

	private const string mFunctionRemoveBuddy = "RemoveBuddy";

	private const string mFunctionUpdateBestBuddy = "UpdateBestBuddy";

	private const string mFunctionHouseDataByUserID = "GetHouseByUserID";

	private const string mFunctionSceneDataByUserID = "GetSceneByUserID";

	private const string mFunctionGetNeighborsByUserID = "GetNeighborsByUserID";

	private const string mFunctionSetNeighbor = "SetNeighbor";

	private const string mFunctionGetActiveParties = "GetActiveParties";

	private const string mFunctionGetPartyByUserID = "GetPartyByUserID";

	private const string mFunctionGetPartiesByUserID = "GetPartiesByUserID";

	private const string mFunctionPurchaseParty = "PurchaseParty";

	private const string mFunctionGetUserRoomItemPositions = "GetUserRoomItemPositions";

	private const string mFunctionSetUserRoomItemPositions = "SetUserRoomItemPositions";

	private const string mFunctionGetUserTimedItems = "GetUserTimedItemByUserID";

	private const string mFunctionSetUserTimedItem = "SetUserTimedItemFlagged";

	private const string mFunctionDeleteUserTimedItem = "DeleteUserTimedItem";

	private const string mFunctionGetUserStaff = "GetUserStaffByUserID";

	private const string mFunctionSetUserStaff = "SetUserStaffFlagged";

	private const string mFunctionGetAuthoritativeTime = "GetAuthoritativeTime";

	private const string mFunctionGetStreamPost = "GetStreamPost";

	private const string mFunctionGetUserFacebookUserMapByList = "GetUserFacebookUserMapByList";

	private const string mFunctionSetStreamPostLog = "SetStreamPostLog";

	private const string mFuncitonGetAllActivePetsByuserId = "GetAllActivePetsByuserId";

	private const string mFunctionGetInactiveRaisedPet = "GetInactiveRaisedPet";

	private const string mFunctionGetActiveRaisedPet = "GetActiveRaisedPet";

	private const string mFunctionGetActiveRaisedPetsByTypes = "GetActiveRaisedPetsByTypes";

	private const string mFunctionGetReleasedRaisedPet = "GetInactiveRaisedPet";

	private const string mFunctionGetSelectedRaisedPet = "GetSelectedRaisedPet";

	private const string mFunctionGetSelectedPetByType = "GetSelectedPetByType";

	private const string mFunctionGetUnselectedPetsByTypes = "GetUnselectedPetByTypes";

	private const string mFunctionSetRaisedPet = "SetRaisedPet";

	private const string mFunctionCreateRaisedPet = "CreateRaisedPet";

	private const string mFunctionCreatePet = "CreatePet";

	private const string mFunctionSetRaisedPetInactive = "SetRaisedPetInactive";

	private const string mFunctionSetSelectedPet = "SetSelectedPet";

	private const string mFunctionGetUserActivity = "GetUserActivityByUserID";

	private const string mFunctionSetUserActivity = "SetUserActivity";

	private const string mFunctionSetRating = "SetRating";

	private const string mFunctionSetRatingForUserID = "SetUserRating";

	private const string mFunctionGetAllRatings = "GetRatingByRatedEntity";

	private const string mFunctionClearRating = "DeleteEntityRating";

	private const string mFunctionGetTopRank = "GetTopRatedByCategoryID";

	private const string mFunctionGetTopRanksWithUserID = "GetTopRatedUserByCategoryID";

	private const string mFunctionGetRank = "GetEntityRatedRank";

	private const string mFunctionGetRatingForRatedUser = "GetRatingForRatedUser";

	private const string mFunctionGetAverageRatingForRoom = "GetAverageRatingForRoom";

	private const string mFunctionGetRatingForRatedEntity = "GetRatingForRatedEntity";

	private const string mFunctionSetScore = "SetScore";

	private const string mFunctionGetTopScore = "GetEntityTopScores";

	private const string mFunctionClearScore = "DeleteEntityScoreData";

	private const string mFunctionGetTracksByUID = "GetTracksByUserID";

	private const string mFunctionGetTracksByIDs = "GetTracksByIDs";

	private const string mFunctionGetTrackElements = "GetTrackElements";

	private const string mFunctionSetTrack = "SetTrack";

	private const string mFunctionDeleteTrack = "DeleteTrack";

	private const string mFunctionRemoveUserWOTrack = "RemoveUserWithoutTrack";

	private const string mFunctionSetUserAchievementTask = "SetUserAchievementTask";

	private const string mFunctionSetAchievementTaskByUserID = "SetAchievementTaskByUserID";

	private const string mFunctionGetUserAchievementTask = "GetUserAchievementTask";

	private const string mFunctionGetLocaleData = "GetLocaleData";

	private const string mFunctionGetGamePlayDataForDateRange = "GetGamePlayDataForDateRange";

	private const string mFunctionGetUserProfile = "GetUserProfile";

	private const string mFunctionGetUserProfileByUserID = "GetUserProfileByUserID";

	private const string mFunctionGetQuestions = "GetQuestions";

	private const string mFunctionGetUserAnswers = "GetUserAnswers";

	private const string mFunctionSetUserProfileAnswers = "SetUserProfileAnswers";

	private const string mFunctionSetUserGender = "SetUserGender";

	private const string mFunctionGetBuddyStatusList = "GetBuddyStatusList";

	private const string mFunctionGetConversationByMessageID = "GetConversationByMessageID";

	private const string mFunctionGetMessageBoard = "GetMessageBoard";

	private const string mFunctionGetStatusConversation = "GetStatusConversation";

	private const string mFunctionRemoveMessageFromBoard = "RemoveMessageFromBoard";

	private const string mFunctionChatAuthorizationRequest = "ChatAuthorizationRequest";

	private const string mFunctionReportUser = "ReportUser";

	private const string mFunctionGetGameProgress = "GetGameProgress";

	private const string mFunctionGetGameProgressByUserID = "GetGameProgressByUserID";

	private const string mFunctionSetGameProgress = "SetGameProgress";

	private const string mFunctionGetTopUsersByCorrectAnswer = "GetTopUsersByCorrectAnswer";

	private const string mFunctionInitiateChallenge = "InitiateChallenge";

	private const string mFunctionAcceptChallenge = "AcceptChallenge";

	private const string mFunctionRejectChallenge = "RejectChallenge";

	private const string mFunctionRespondToChallenge = "RespondToChallenge";

	private const string mFunctionGetActiveChallenges = "GetActiveChallenges";

	private const string mFunctionAcceptTrade = "AcceptTrade";

	private const string mFunctionLoginParent = "LoginParent";

	private const string mFunctionLoginChild = "LoginChild";

	private const string mFunctionLoginGuest = "LoginGuest";

	private const string mFunctionRecoverPassword = "RecoverPassword";

	private const string mFunctionResetPassword = "ResetPassword";

	private const string mFunctionRecoverAccount = "RecoverAccount";

	private const string mFunctionRegisterParent = "RegisterParent";

	private const string mFunctionRegisterChild = "RegisterChild";

	private const string mFunctionDeleteProfile = "DeleteProfile";

	private const string mFunctionConvertGuest = "ConvertGuest";

	private const string mFunctionRegisterAppInstall = "RegisterAppInstall";

	private const string mFunctionSetUserLogoff = "SetUserLogoff";

	private const string mFunctionGetUsersWithPet = "GetUsersWithPets";

	private const string mFunctionIsValidApiToken_V2 = "IsValidApiToken_V2";

	private const string mFunctionSendAccountActivationReminder = "SendAccountActivationReminder";

	public const string mFunctionGetMMOServerInfo = "GetMMOServerInfoWithZone";

	public const string mFunctionAuthenticateUser = "AuthenticateUser";

	public const string mFunctionCheckProductVersion = "CheckProductVersion";

	public const string mFunctionSendFriendInvite = "SendFriendInvite";

	public const string mFunctionSendFriendInviteRequest = "SendFriendInviteRequest";

	public const string mFunctionFriendInviteRegisterRequest = "PostFriendInviteRegisterProcessing";

	private const string mFunctionGetProfileTagAll = "GetProfileTagAll";

	private const string mFunctionPurchaseGift = "PurchaseGift";

	private const string mFunctionGetCombinedListMessage = "GetCombinedListMessage";

	private const string mFunctionSetTaskState = "SetTaskState";

	private const string mFunctionSetTimedMissionTaskState = "SetTimedMissionTaskState";

	private const string mFunctionSetNickname = "SetNickname";

	private const string mFunctionGetNickname = "GetNickname";

	private const string mFunctionGetNicknames = "GetNicknames";

	private const string mFunctionSetNextItemState = "SetNextItemState";

	private const string mFunctionSetSpeedUpItem = "SetSpeedUpItem";

	private const string mFunctionCreateGroup = "CreateGroup";

	private const string mFunctionEditGroup = "EditGroup";

	private const string mFunctionJoinGroup = "JoinGroup";

	private const string mFunctionLeaveGroup = "LeaveGroup";

	private const string mFunctionGetGroups = "GetGroups";

	private const string mFunctionRemoveMember = "RemoveMember";

	private const string mFunctionAuthorizeJoinRequest = "AuthorizeJoinRequest";

	private const string mFunctionAssignRole = "AssignRole";

	private const string mFunctionGetPendingJoinRequest = "GetPendingJoinRequest";

	private const string mFunctionInvitePlayer = "InvitePlayer";

	private const string mFunctionUpdateInvite = "UpdateInvite";

	private const string mFunctionGetMembersByGroupID = "GetMembersByGroupID";

	private const string mFunctionGetGroupsByUserID = "GetGroupsByUserID";

	private const string mFunctionGetGroupsByGroupType = "GetGroupsByGroupType";

	private const string mFunctionValidateName = "ValidateName";

	private const string mFunctionRedeemItem = "RedeemItems";

	private const string mFunctionRedeemMysteryBoxItems = "RedeemMysteryBoxItems";

	private const string mFunctionRedeemReceipt = "RedeemReceipt";

	private const string mFunctionProcessSteam = "ProcessSteam";

	private const string mFunctionGetXSollaToken = "GetToken";

	private const string mFunctionCreatePurchaseOrder = "CreatePurchaseOrder";

	private const string mFunctionValidatePrizeCode = "ValidatePrizeCode";

	private const string mFunctionSubmitPrizeCode = "SubmitPrizeCode";

	private const string mFunctionGetUserRoomList = "GetUserRoomList";

	private const string mFunctionSetUserRoom = "SetUserRoom";

	private const string mFunctionApplyRewards = "ApplyRewards";

	private const string mFunctionProcessRewards = "ProcessRewardedItems";

	private const string mFunctionFuseItems = "FuseItems";

	private const string mFunctionAddBattleItems = "AddBattleItems";

	private const string mFunctionItemExchange = "ProcessExchangeItem";

	private const string mFunctionGetAllExchangeItems = "GetAllExchangeItemList";

	private const string mFunctionGetExchangeItemListByItem = "GetExchangeItemListByItem";

	private static ApiTokenStatus mTokenStatus = ApiTokenStatus.TokenValid;

	private static long Ticks => _Ticks++;

	public static string pSecret => mSecret;

	public static string pUserToken => mUserToken;

	public static string ApiKey => mApiKey;

	public static ApiTokenStatus pTokenStatus
	{
		get
		{
			return mTokenStatus;
		}
		set
		{
			mTokenStatus = value;
		}
	}

	public static void SetToken(string token)
	{
		mUserToken = token.ToLower();
	}

	public static void InitForm(WWWForm inForm)
	{
		inForm.AddField("apiToken", mUserToken);
		inForm.AddField("apiKey", mApiKey);
	}

	public static void AddCommon(ServiceRequest inRequest)
	{
		if (inRequest != null)
		{
			inRequest.AddParam("apiToken", mUserToken);
			inRequest.AddParam("apiKey", mApiKey);
		}
	}

	public static void Init()
	{
		mContentURL = ProductConfig.pInstance.ContentServerURL;
		mContentV2URL = ProductConfig.pInstance.ContentServerV2URL;
		mContentV3URL = ProductConfig.pInstance.ContentServerV3URL;
		mContentV4URL = ProductConfig.pInstance.ContentServerV4URL;
		mMembershipURL = ProductConfig.pInstance.MembershipServerURL;
		mConfigurationURL = ProductConfig.pInstance.ConfigurationServerURL;
		mAuthenticationURL = ProductConfig.pInstance.AuthenticationServerURL;
		mAuthenticationV3URL = ProductConfig.pInstance.AuthenticationServerV3URL;
		mAvatarURL = ProductConfig.pInstance.AvatarWebServiceURL;
		mMessagingURL = ProductConfig.pInstance.MessagingServerURL;
		mAchievementURL = ProductConfig.pInstance.AchievementServerURL;
		mAchievementV2URL = ProductConfig.pInstance.AchievementServerV2URL;
		mItemStoreURL = ProductConfig.pInstance.ItemStoreServerURL;
		mMissionURL = ProductConfig.pInstance.MissionServerURL;
		mSubscriptionURL = ProductConfig.pInstance.SubscriptionServerURL;
		mFacebookURL = ProductConfig.pInstance.FacebookServerURL;
		mRatingURL = ProductConfig.pInstance.RatingServerURL;
		mRatingV2URL = ProductConfig.pInstance.RatingServerV2URL;
		mScoreURL = ProductConfig.pInstance.ScoreServerURL;
		mTrackURL = ProductConfig.pInstance.TrackServerURL;
		mLocaleURL = ProductConfig.pInstance.LocaleServerURL;
		mLocaleV2URL = ProductConfig.pInstance.LocaleServerV2URL;
		mProfileURL = ProductConfig.pInstance.UserServerURL;
		mMessageURL = ProductConfig.pInstance.MessageServerURL;
		mMessageV2URL = ProductConfig.pInstance.MessageServerV2URL;
		mMessageV3URL = ProductConfig.pInstance.MessageServerV3URL;
		mChatURL = ProductConfig.pInstance.ChatServerURL;
		mChallengeURL = ProductConfig.pInstance.ChallengeServerURL;
		mRegistrationV3URL = ProductConfig.pInstance.RegistrationServerV3URL;
		mRegistrationV4URL = ProductConfig.pInstance.RegistrationServerV4URL;
		mInviteV2URL = ProductConfig.pInstance.InviteServerV2URL;
		mPushNotificationURL = ProductConfig.pInstance.PushNotificationURL;
		mGroupURL = ProductConfig.pInstance.GroupServerURL;
		mGroupV2URL = ProductConfig.pInstance.GroupServerV2URL;
		mPaymentURL = ProductConfig.pInstance.PaymentServerURL;
		mPaymentV2URL = ProductConfig.pInstance.PaymentServerV2URL;
		mPrizeCodeURL = ProductConfig.pInstance.PrizeCodeServerURL;
		mPrizeCodeV2URL = ProductConfig.pInstance.PrizeCodeServerV2URL;
		mUserToken = ProductConfig.pToken;
		mApiKey = ProductConfig.pApiKey;
		mSecret = ProductConfig.pSecret;
		mProductID = ProductConfig.pProductID;
		mUserToken = mUserToken.ToLower();
		mApiKey = mApiKey.ToLower();
		_Ticks = DateTime.UtcNow.Ticks;
	}

	public static void GetAvatarDisplayDataByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_AVATAR_DISPLAY_DATA_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetAvatarDisplayDataByUserID";
		obj.AddParam("userId", userID);
		ServiceCall<AvatarDisplayData>.Create(obj)?.DoGet();
	}

	public static void GetAvatarDisplayData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_AVATAR_DISPLAY_DATA,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetAvatarDisplayData";
		ServiceCall<AvatarDisplayData>.Create(obj)?.DoGet();
	}

	public static void GetAvatarByUserID(string inUserID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_AVATAR_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", inUserID);
		obj._URL = mContentURL + "GetAvatarByUserID";
		ServiceCall<AvatarData>.Create(obj)?.DoGet();
	}

	public static void GetAvatarData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_AVATAR,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetAvatar";
		ServiceCall<AvatarData>.Create(obj)?.DoGet();
	}

	public static void SetAvatar(AvatarData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_AVATAR,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "SetAvatar";
		ServiceCall<SetAvatarResult>.Create(obj)?.DoSet();
	}

	public static void GetDisplayNameByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_DISPLAYNAME_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetDisplayNameByUserID";
		ServiceCall<string>.Create(obj)?.DoGet();
	}

	public static void GetDefaultNameSuggestion(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_DEFAULT_NAME_SUGGESTION,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetDefaultNameSuggestion";
		ServiceCall<DisplayNameUniqueResponse>.Create(obj)?.DoGet();
	}

	public static void GetUserRoomList(string userID, int? categoryID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ROOM_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(new UserRoomGetRequest
		{
			UserID = new Guid(userID),
			CategoryID = categoryID
		});
		obj.AddParam("request", inVal);
		obj._URL = mContentURL + "GetUserRoomList";
		ServiceCall<UserRoomResponse>.Create(obj)?.DoGet();
	}

	public static void SetUserRoom(UserRoom inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_ROOM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj.AddParam("request", text);
		obj._URL = mContentURL + "SetUserRoom";
		ServiceCall<UserRoomSetResponse>.Create(obj)?.DoSet();
	}

	public static void GetInventoryData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_INVENTORY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetInventory";
		ServiceCall<InventoryData>.Create(obj)?.DoGet();
	}

	public static void SetInventoryData(InventoryData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_INVENTORY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetInventory";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetCommonInventoryData(int inContainerid, WsServiceEventHandler inCallback, object inUserData)
	{
		GetCommonInventoryData(mUserToken, inContainerid, inCallback, inUserData);
	}

	public static void GetCommonInventoryData(string inToken, int inContainerid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_COMMON_INVENTORY;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiToken", inToken);
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("ContainerId", inContainerid);
		serviceRequest._URL = mContentURL + "GetCommonInventory";
		ServiceCall<CommonInventoryData>.Create(serviceRequest)?.DoGet();
	}

	public static void GetCommonInventoryData(int inContainerid, bool loadItemStats, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_COMMON_INVENTORY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(new GetCommonInventoryRequest
		{
			ContainerId = inContainerid,
			LoadItemStats = loadItemStats,
			Locale = UtUtilities.GetLocaleLanguage()
		});
		obj.AddParam("getCommonInventoryRequestXml", inVal);
		obj._URL = mContentV2URL + "GetCommonInventory";
		ServiceCall<CommonInventoryData>.Create(obj)?.DoGet();
	}

	public static void GetCommonInventoryDataByUserID(string userId, int inContainerid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_COMMON_INVENTORY_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("ContainerId", inContainerid);
		obj.AddParam("userId", userId);
		obj._URL = mContentURL + "GetCommonInventoryByUserID";
		ServiceCall<CommonInventoryData>.Create(obj)?.DoGet();
	}

	public static void SetCommonInventoryData(CommonInventoryRequest[] inRequest, int inContainerid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_COMMON_INVENTORY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(inRequest);
		serviceRequest.AddParam("commonInventoryRequestXml", text);
		serviceRequest.AddParam("ContainerId", inContainerid);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text + inContainerid));
		serviceRequest._URL = mContentURL + "SetCommonInventory";
		ServiceCall<CommonInventoryResponse>.Create(serviceRequest)?.DoSet();
	}

	public static void SetCommonInventoryAttribute(int inventoryID, PairData pairData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_COMMON_INVENTORY_ATTRIBUTE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(pairData);
		serviceRequest.AddParam("commonInventoryID", inventoryID);
		serviceRequest.AddParam("pairxml", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + inventoryID + text));
		serviceRequest._URL = mContentURL + "SetCommonInventoryAttribute";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void RerollUserItem(RollUserItemRequest rollUserItemRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REROLL_USER_ITEM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(rollUserItemRequest);
		obj.AddParam("request", text);
		obj._URL = mContentV2URL + "RerollUserItem";
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		ServiceCall<RollUserItemResponse>.Create(obj)?.DoGet();
	}

	public static void PurchaseItems(PurchaseStoreItemRequest purchaseItemRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.PURCHASE_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(purchaseItemRequest);
		obj.AddParam("purchaseItemRequest", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "PurchaseItems";
		ServiceCall<CommonInventoryResponse>.Create(obj)?.DoGet();
	}

	public static void PurchaseItems(int[] itemIDs, int currencyType, int inContainerid, int storeid, WsServiceEventHandler inCallback, object inUserData)
	{
		PurchaseItems(mUserToken, itemIDs, currencyType, inContainerid, storeid, inCallback, inUserData);
	}

	public static void PurchaseItems(string inUserToken, int[] itemIDs, int currencyType, int inContainerid, int storeid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.PURCHASE_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		serviceRequest.AddParam("apiToken", inUserToken);
		serviceRequest.AddParam("apiKey", mApiKey);
		string text = UtUtilities.ProcessSendObject(itemIDs);
		serviceRequest.AddParam("itemIDArrayXml", text);
		serviceRequest.AddParam("currencyType", currencyType);
		serviceRequest.AddParam("storeId", storeid);
		serviceRequest.AddParam("ContainerId", inContainerid);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + inUserToken + storeid + text + inContainerid + currencyType));
		serviceRequest._URL = mContentURL + "PurchaseItems";
		ServiceCall<CommonInventoryResponse>.Create(serviceRequest)?.DoGet();
	}

	public static void RedeemItem(RedeemRequest redeemRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REDEEM_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(redeemRequest);
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "RedeemItems";
		ServiceCall<CommonInventoryResponse>.Create(obj)?.DoGet();
	}

	public static void RedeemItems(CommonInventory.V4.RedeemRequest[] redeemRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REDEEM_MULTIPLE_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(redeemRequest);
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV4URL + "RedeemItems";
		ServiceCall<CommonInventoryGroupResponse>.Create(obj)?.DoGet();
	}

	public static void RedeemMysteryBoxItems(RedeemRequest redeemRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REDEEM_MYSTERY_BOX_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(redeemRequest);
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "RedeemMysteryBoxItems";
		ServiceCall<CommonInventoryResponse>.Create(obj)?.DoGet();
	}

	public static void SellItems(SellItemsRequest sellItemsRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SELL_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(sellItemsRequest);
		obj.AddParam("sellItemsRequest", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "SellItems";
		ServiceCall<CommonInventoryResponse>.Create(obj)?.DoSet();
	}

	public static void UseCommonInventoryData(int inventoryId, int numOfUse, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.USE_INVENTORY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userInventoryId", inventoryId);
		serviceRequest.AddParam("numberOfUses", numOfUse);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + inventoryId + numOfUse));
		serviceRequest._URL = mContentURL + "UseInventory";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetProductData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_PRODUCT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetProduct";
		ServiceCall<ProductData>.Create(obj)?.DoGet();
	}

	public static void SetProductData(ProductData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_PRODUCT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetProduct";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetSceneData(string inSceneName, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_SCENE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("sceneName", inSceneName);
		obj._URL = mContentURL + "GetScene";
		ServiceCall<SceneData>.Create(obj)?.DoGet();
	}

	public static void GetSceneDataByUserID(string userID, string inSceneName, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_SCENE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("sceneName", inSceneName);
		obj.AddParam("productId", mProductID);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetSceneByUserID";
		ServiceCall<SceneData>.Create(obj)?.DoGet();
	}

	public static void SetSceneData(string inSceneName, SceneData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_SCENE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("sceneName", inSceneName);
		string text = UtUtilities.ProcessSendObject(inData);
		serviceRequest.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + inSceneName + text));
		serviceRequest._URL = mContentURL + "SetScene";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetHouseData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_HOUSE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetHouse";
		ServiceCall<HouseData>.Create(obj)?.DoGet();
	}

	public static void GetHouseDataByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_HOUSE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("productId", mProductID);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetHouseByUserID";
		ServiceCall<HouseData>.Create(obj)?.DoGet();
	}

	public static void SetHouseData(HouseData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_HOUSE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetHouse";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetMasteryData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_LESSON_MASTERY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetLessonMastery";
		ServiceCall<MasteryData>.Create(obj)?.DoGet();
	}

	public static void SetMasteryData(MasteryData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_LESSON_MASTERY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetLessonMastery";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void SetLessonData(LessonData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_STUDENT_STATUS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetLessonStatus";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void DeleteImageData(string inImageType, int inSlot, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.DEL_IMAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("ImageType", inImageType);
		serviceRequest.AddParam("ImageSlot", inSlot);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + inImageType + inSlot));
		serviceRequest._URL = mContentURL + "DelImage";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetImageData(string inImageType, int inSlot, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_IMAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("ImageType", inImageType);
		obj.AddParam("ImageSlot", inSlot);
		obj._URL = mContentURL + "GetImage";
		ServiceCall<ImageData>.Create(obj)?.DoGet();
	}

	public static void GetImageDataByUserId(string inUserId, string inImageType, int inSlot, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_IMAGE_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", inUserId);
		obj.AddParam("ImageType", inImageType);
		obj.AddParam("ImageSlot", inSlot);
		obj._URL = mContentURL + "GetImageByUserId";
		ServiceCall<ImageData>.Create(obj)?.DoGet();
	}

	public static void GetImageByProductGroup(string inUserId, string inImageType, int inSlot, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_IMAGE_BY_PRODUCT_GROUP,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", inUserId);
		obj.AddParam("ImageType", inImageType);
		obj.AddParam("ImageSlot", inSlot);
		obj._URL = mContentURL + "GetImageByProductGroup";
		ServiceCall<ImageDataComplete>.Create(obj)?.DoGet();
	}

	public static void SetImageData(string inImageType, int inSlot, Texture2D inImage, ImageData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_IMAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("ImageType", inImageType);
		serviceRequest.AddParam("ImageSlot", inSlot);
		string text = UtUtilities.ProcessSendObject(inData);
		serviceRequest.AddParam("contentXML", text);
		string text2 = Convert.ToBase64String(inImage.EncodeToJPG(25));
		serviceRequest.AddParam("imageFile", text2);
		string text3 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text3);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text3 + mSecret + mUserToken + inImageType + inSlot + text + text2));
		serviceRequest._URL = mContentURL + "SetImage";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void SetImagesData(ImageSetRequests inSetImages, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_IMAGES,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(inSetImages);
		obj.AddParam("imageSetRequests", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "SetImages";
		ServiceCall<SetImagesResponse>.Create(obj)?.DoGet();
	}

	public static void DeleteCurrentPetData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.DEL_CURRENT_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken));
		obj._URL = mContentURL + "DelCurrentPet";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetCurrentPetData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_CURRENT_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetCurrentPet";
		ServiceCall<PetData>.Create(obj)?.DoGet();
	}

	public static void GetCurrentPetByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_CURRENT_PET_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetCurrentPetByUserID";
		ServiceCall<PetData>.Create(obj)?.DoGet();
	}

	public static void SetCurrentPetData(PetData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_CURRENT_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetCurrentPet";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetAdoptedPetData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ADOPTED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetAdoptedPet";
		ServiceCall<PetData>.Create(obj)?.DoGet();
	}

	public static void SetAdoptedPetData(PetData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_ADOPTED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inData);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetAdoptedPet";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetSubscriptionInfo(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_SUBSCRIPTION_INFO,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mMembershipURL + "GetSubscriptionInfo";
		ServiceCall<SubscriptionInfo>.Create(obj)?.DoGet();
	}

	public static void ExtendMembership(string userID, bool memberOnly, int numDays, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.EXTEND_MEMBERSHIP,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("memberOnly", memberOnly.ToString());
		serviceRequest.AddParam("numberOfDays", numDays);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + memberOnly + numDays));
		serviceRequest._URL = mSubscriptionURL + "ExtendMembership";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetChildList(string parentApiToken, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_CHILD_LIST;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("apiToken", parentApiToken);
		serviceRequest._URL = mMembershipURL + "GetChildList";
		ServiceCall<ChildListInfo>.Create(serviceRequest)?.DoGet();
	}

	public static void GetDetailedChildList(string parentApiToken, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_DETAILED_CHILD_LIST;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("parentApiToken", parentApiToken);
		serviceRequest._URL = mProfileURL + "GetDetailedChildList";
		ServiceCall<UserProfileDataList>.Create(serviceRequest)?.DoGet();
	}

	public static void GetConfigurationSetting(string key, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_CONFIGURATION_SETTING,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("productId", mProductID);
		obj.AddParam("key", key);
		obj._URL = mConfigurationURL + "GetConfigurationSetting";
		ServiceCall<ConfigurationSetting>.Create(obj)?.DoGet();
	}

	public static void GetConfigurationSettings(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_CONFIGURATION_SETTINGS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("productId", mProductID);
		obj._URL = mConfigurationURL + "GetConfigurationSettings";
		ServiceCall<ConfigurationSettings>.Create(obj)?.DoGet();
	}

	public static void GetContentByType(int type, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_CONTENT_BY_TYPE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("contentType", type);
		obj._URL = mConfigurationURL + "GetContentByTypeByUser";
		ServiceCall<ArrayOfContentInfo>.Create(obj)?.DoGet();
	}

	public static void GetUserInfoByApiToken(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USERINFO_BY_TOKEN,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAuthenticationURL + "GetUserInfoByApiToken";
		ServiceCall<UserInfo>.Create(obj)?.DoGet();
	}

	public static void GetValidatedUserID(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_VALIDATED_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAuthenticationURL + "GetValidatedUserID";
		ServiceCall<Guid>.Create(obj)?.DoGet();
	}

	public static void CreateAvatarContestEntry(Texture2D inImage, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.CREATE_AVATAR_CONTEST_ENTRY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		int num = 5;
		obj.AddParam("imageFormatTypeID", num);
		string inVal = Convert.ToBase64String(inImage.EncodeToPNG());
		obj.AddParam("base64StringData", inVal);
		obj._URL = mAvatarURL + "CreateAvatarContestEntry";
		ServiceCall<object>.Create(obj)?.DoGet();
	}

	public static void GetAllRewardTypeMultiplier(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ALL_REWARD_TYPE_MULTIPLIER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAchievementURL + "GetAllRewardTypeMultiplier";
		ServiceCall<ArrayOfRewardTypeMultiplier>.Create(obj)?.DoGet();
	}

	public static void GetAllRanks(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ALL_RANKS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAchievementURL + "GetAllRanks";
		ServiceCall<ArrayOfUserRank>.Create(obj)?.DoGet();
	}

	public static void GetRankByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_RANK_BY_USERID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mAchievementURL + "GetRankByUserID";
		ServiceCall<UserRankData>.Create(obj)?.DoGet();
	}

	public static void GetRankByUserIDs(Guid[] userIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_RANK_BY_USERIDS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(userIDs);
		obj.AddParam("userIds", inVal);
		obj._URL = mAchievementURL + "GetRankByUserIDs";
		ServiceCall<ArrayOfUserRankData>.Create(obj)?.DoGet();
	}

	public static void GetRankAttributeData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_RANK_ATTRIBUTE_DATA,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mItemStoreURL + "GetRankAttributeData";
		ServiceCall<ArrayOfRankAttributeData>.Create(obj)?.DoGet();
	}

	public static void GetUserAchievementInfo(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ACHIEVEMENT_INFO,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAchievementURL + "GetUserAchievementInfo";
		ServiceCall<UserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void GetUserAchievements(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ACHIEVEMENTS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAchievementURL + "GetUserAchievements";
		ServiceCall<ArrayOfUserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void GetUserAchievementTaskRedeemableRewards(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ACHIEVEMENTS_TASK_REDEEMABLE_REWARDS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAchievementV2URL + "GetUserAchievementTaskRedeemableRewards";
		ServiceCall<UserAchievementTaskRedeemableRewards>.Create(obj)?.DoGet();
	}

	public static void RedeemUserAchievementTaskReward(int achievementInfoID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REDEEM_USER_ACHIEVEMENT_TASK_REWARD,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("achievementInfoID", achievementInfoID);
		obj._URL = mAchievementV2URL + "RedeemUserAchievementTaskReward";
		ServiceCall<RedeemUserAchievementTaskResponse>.Create(obj)?.DoGet();
	}

	public static void GetAchievementsByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACHIEVEMENTS_BY_USERID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mAchievementURL + "GetAchievementsByUserID";
		ServiceCall<ArrayOfUserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void GetAchievementTaskInfo(int[] taskIdArray, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACHIEVEMENT_TASK_INFO,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.SerializeToXml(taskIdArray);
		obj.AddParam("achievementTaskIDList", inVal);
		obj._URL = mAchievementURL + "GetAchievementTaskInfo";
		ServiceCall<ArrayOfAchievementTaskInfo>.Create(obj)?.DoGet();
	}

	public static void GetPetAchievementsByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_PET_ACHIEVEMENTS_BY_USERID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mAchievementURL + "GetPetAchievementsByUserID";
		ServiceCall<ArrayOfUserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void SetUserAchievement(int achievementId, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_ACHIEVEMENT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("achievementID", achievementId);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + achievementId));
		obj._URL = mAchievementURL + "SetUserAchievement";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void SetUserAchievementAndGetReward(int achievementId, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_ACHIEVEMENT_AND_GET_REWARD,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("achievementID", achievementId);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + achievementId));
		obj._URL = mAchievementURL + "SetUserAchievementAndGetReward";
		ServiceCall<AchievementReward[]>.Create(obj)?.DoGet();
	}

	public static void SetAchievementAndGetReward(int achievementId, string inGroupID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_ACHIEVEMENT_AND_GET_REWARD,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("groupID", inGroupID);
		serviceRequest.AddParam("achievementID", achievementId);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + achievementId + inGroupID));
		serviceRequest._URL = mAchievementURL + "SetAchievementAndGetReward";
		ServiceCall<AchievementReward[]>.Create(serviceRequest)?.DoGet();
	}

	public static void SetAchievementByEntityIDs(int achievementId, Guid?[] petIDs, string inGroupID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_ACHIEVEMENT_BY_ENTITY_IDS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string empty = string.Empty;
		empty = UtUtilities.ProcessSendObject(petIDs);
		serviceRequest.AddParam("petIDs", empty);
		serviceRequest.AddParam("groupID", inGroupID);
		serviceRequest.AddParam("achievementID", achievementId);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + achievementId + inGroupID));
		serviceRequest._URL = mAchievementURL + "SetAchievementByEntityIDs";
		ServiceCall<AchievementReward[]>.Create(serviceRequest)?.DoGet();
	}

	public static void ApplyPayout(string inModuleName, int inPoints, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.APPLY_PAYOUT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("ModuleName", inModuleName);
		serviceRequest.AddParam("points", inPoints);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + inModuleName + inPoints));
		serviceRequest._URL = mAchievementURL + "ApplyPayout";
		ServiceCall<AchievementReward[]>.Create(serviceRequest)?.DoGet();
	}

	public static void GetTopAchievementPointUsers(int pageIndex, int userInPage, int userMode, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_ACHIEVEMENT_POINT_USERS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("page", pageIndex);
		obj.AddParam("quantity", userInPage);
		obj.AddParam("modeID", userMode);
		obj._URL = mAchievementURL + "GetTopAchievementPointUsers";
		ServiceCall<ArrayOfUserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void GetTopAchievementPointBuddies(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_ACHIEVEMENT_POINT_BUDDIES,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mAchievementURL + "GetTopAchievementPointBuddies";
		ServiceCall<ArrayOfUserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void GetTopAchievementPointUsersByType(int pageIndex, int userInPage, int userMode, int pointTypeID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_ACHIEVEMENT_POINT_USERS_BY_TYPE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("page", pageIndex);
		obj.AddParam("quantity", userInPage);
		obj.AddParam("modeID", userMode);
		obj.AddParam("pointTypeID", pointTypeID);
		obj._URL = mAchievementURL + "GetTopAchievementPointUsersByType";
		ServiceCall<ArrayOfUserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void GetTopAchievementPointBuddiesByType(string userID, int pointTypeID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_ACHIEVEMENT_POINT_BUDDIES_BY_TYPE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("pointTypeID", pointTypeID);
		obj._URL = mAchievementURL + "GetTopAchievementPointBuddiesByType";
		ServiceCall<ArrayOfUserAchievementInfo>.Create(obj)?.DoGet();
	}

	public static void GetTopAchievementPointUsers(string userID, int pageIndex, int usersPerPage, RequestType typeID, ModeType modeID, int pointTypeID, List<long> facebookFriends, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_ACHIEVEMENT_POINT_USERS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.SerializeToXml(new UserAchievementInfoRequest
		{
			UserID = new Guid(userID),
			Type = typeID,
			Mode = modeID,
			PointTypeID = pointTypeID,
			Page = pageIndex,
			Quantity = usersPerPage,
			FacebookUserIDs = facebookFriends
		});
		obj.AddParam("request", inVal);
		obj._URL = mAchievementV2URL + "GetTopAchievementPointUsers";
		ServiceCall<UserAchievementInfoResponse>.Create(obj)?.DoGet();
	}

	public static void GetAchievementRewardsByAchievementTaskID(int inAchievementTaskID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACHIEVEMENT_REWARDS_BY_ACHIEVEMENT_TASKID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("achievementTaskId", inAchievementTaskID);
		obj._URL = mAchievementV2URL + "GetAchievementRewardsByAchievementTaskID";
		ServiceCall<AchievementReward[]>.Create(obj)?.DoGet();
	}

	public static void GetAnnouncements(int worldObjectID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ANNOUNCEMENTS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("worldObjectID", worldObjectID);
		obj._URL = mItemStoreURL + "GetAnnouncementsByUser";
		ServiceCall<AnnouncementList>.Create(obj)?.DoGet();
	}

	public static void GetUserMessageQueue(bool showOld, bool showDeleted, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_MESSAGE_QUEUE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("showOldMessages", showOld.ToString());
		obj.AddParam("showDeletedMessages", showDeleted.ToString());
		obj._URL = mMessagingURL + "GetUserMessageQueue";
		ServiceCall<ArrayOfMessageInfo>.Create(obj)?.DoGet();
	}

	public static void SaveMessage(int userMessageQueueID, bool isNew, bool isDeleted, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SAVE_MESSAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userMessageQueueID", userMessageQueueID.ToString());
		serviceRequest.AddParam("isNew", isNew.ToString());
		serviceRequest.AddParam("isDeleted", isDeleted.ToString());
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userMessageQueueID + isNew + isDeleted));
		serviceRequest._URL = mMessagingURL + "SaveMessage";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void SendMessage(string toUserID, int messageID, Dictionary<string, string> inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SEND_MESSAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("toUser", toUserID);
		serviceRequest.AddParam("messageID", messageID.ToString());
		List<System.Collections.Generic.KeyValuePair<string, string>> list = new List<System.Collections.Generic.KeyValuePair<string, string>>();
		if (inData != null)
		{
			foreach (System.Collections.Generic.KeyValuePair<string, string> inDatum in inData)
			{
				list.Add(inDatum);
			}
		}
		string text = UtUtilities.ProcessSendObject(list);
		serviceRequest.AddParam("data", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + toUserID + messageID + text));
		serviceRequest._URL = mMessagingURL + "SendMessage";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void SendMessage(string toUserID, int messageID, SerializableDictionary.KeyValuePair<string, string> inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SEND_MESSAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("toUser", toUserID);
		serviceRequest.AddParam("messageID", messageID.ToString());
		List<SerializableDictionary.KeyValuePair<string, string>> list = new List<SerializableDictionary.KeyValuePair<string, string>>();
		if (inData != null)
		{
			foreach (System.Collections.Generic.KeyValuePair<string, string> inDatum in inData)
			{
				SerializableDictionary.KeyValuePair<string, string> keyValuePair = new SerializableDictionary.KeyValuePair<string, string>();
				keyValuePair.Add(inDatum.Key, inDatum.Value);
				list.Add(keyValuePair);
			}
		}
		string text = UtUtilities.ProcessSendObject(list);
		serviceRequest.AddParam("data", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + toUserID + messageID + text));
		serviceRequest._URL = mMessagingURL + "SendMessage";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void QueueMessage(string fromUserID, string toUserID, int messageID, SerializableDictionary.KeyValuePair<string, string> inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.QUEUE_MESSAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("toUser", toUserID);
		serviceRequest.AddParam("fromUser", fromUserID);
		serviceRequest.AddParam("messageID", messageID.ToString());
		List<SerializableDictionary.KeyValuePair<string, string>> list = new List<SerializableDictionary.KeyValuePair<string, string>>();
		if (inData != null)
		{
			foreach (System.Collections.Generic.KeyValuePair<string, string> inDatum in inData)
			{
				SerializableDictionary.KeyValuePair<string, string> keyValuePair = new SerializableDictionary.KeyValuePair<string, string>();
				keyValuePair.Add(inDatum.Key, inDatum.Value);
				list.Add(keyValuePair);
			}
		}
		string text = UtUtilities.ProcessSendObject(list);
		serviceRequest.AddParam("data", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + toUserID + messageID + text));
		serviceRequest._URL = mMessagingURL + "QueueMessage";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void SendMessageBulk(Guid[] toUserIDs, int messageID, SerializableDictionary.KeyValuePair<string, string> inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SEND_MESSAGE_BULK,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(toUserIDs);
		serviceRequest.AddParam("toUsers", text);
		serviceRequest.AddParam("messageID", messageID.ToString());
		List<SerializableDictionary.KeyValuePair<string, string>> list = new List<SerializableDictionary.KeyValuePair<string, string>>();
		if (inData != null)
		{
			foreach (System.Collections.Generic.KeyValuePair<string, string> inDatum in inData)
			{
				SerializableDictionary.KeyValuePair<string, string> keyValuePair = new SerializableDictionary.KeyValuePair<string, string>();
				keyValuePair.Add(inDatum.Key, inDatum.Value);
				list.Add(keyValuePair);
			}
		}
		string text2 = UtUtilities.ProcessSendObject(list);
		serviceRequest.AddParam("data", text2);
		string text3 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text3);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text3 + mSecret + mUserToken + text + messageID + text2));
		serviceRequest._URL = mMessagingURL + "SendMessageBulk";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void PostNewMessageToBoard(MessageRequest messagerequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = null;
		serviceRequest = ((messagerequest.level != MessageLevel.WhiteList) ? new ServiceRequest
		{
			_Type = WsServiceType.POST_MESSAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		} : new ServiceRequest
		{
			_Type = WsServiceType.POST_MESSAGE_RMF,
			_EventDelegate = inCallback,
			_UserData = inUserData
		});
		AddCommon(serviceRequest);
		string text = UtUtilities.SerializeToXml(messagerequest);
		serviceRequest.AddParam("messagerequest", text);
		if (messagerequest.level == MessageLevel.WhiteList)
		{
			string text2 = Ticks.ToString();
			serviceRequest.AddParam("ticks", text2);
			serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
			serviceRequest._URL = mMessageV3URL + "PostNewMessageToBoardWithRMF";
			ServiceCall<RMFMessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
		else
		{
			serviceRequest._URL = mMessageURL + "PostNewMessageToBoard";
			ServiceCall<MessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
	}

	public static void PostNewMessageToList(string message, MessageLevel level, int targetList, string displayAttribute, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = null;
		serviceRequest = ((level != MessageLevel.WhiteList) ? new ServiceRequest
		{
			_Type = WsServiceType.POST_MESSAGE_TO_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		} : new ServiceRequest
		{
			_Type = WsServiceType.POST_MESSAGE_TO_LIST_RMF,
			_EventDelegate = inCallback,
			_UserData = inUserData
		});
		AddCommon(serviceRequest);
		serviceRequest.AddParam("content", message);
		serviceRequest.AddParam("level", level.ToString());
		serviceRequest.AddParam("targetList", targetList.ToString());
		serviceRequest.AddParam("displayAttribute", displayAttribute);
		if (level == MessageLevel.WhiteList)
		{
			string text = Ticks.ToString();
			serviceRequest.AddParam("ticks", text);
			serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + message + level.ToString() + targetList + displayAttribute));
			serviceRequest._URL = mMessageV2URL + "PostNewMessageToListWithRMF";
			ServiceCall<RMFMessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
		else
		{
			serviceRequest._URL = mMessageURL + "PostNewMessageToList";
			ServiceCall<MessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
	}

	public static void PostNewGroupMessageToList(string groupID, string message, MessageLevel level, int targetList, string displayAttribute, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = null;
		serviceRequest = ((level != MessageLevel.WhiteList) ? new ServiceRequest
		{
			_Type = WsServiceType.POST_GROUP_MESSAGE_TO_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		} : new ServiceRequest
		{
			_Type = WsServiceType.POST_GROUP_MESSAGE_TO_LIST_RMF,
			_EventDelegate = inCallback,
			_UserData = inUserData
		});
		AddCommon(serviceRequest);
		serviceRequest.AddParam("groupID", groupID);
		serviceRequest.AddParam("content", message);
		serviceRequest.AddParam("level", level.ToString());
		serviceRequest.AddParam("targetList", targetList.ToString());
		serviceRequest.AddParam("displayAttribute", displayAttribute);
		if (level == MessageLevel.WhiteList)
		{
			string text = Ticks.ToString();
			serviceRequest.AddParam("ticks", text);
			serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + groupID + message + level.ToString() + targetList + displayAttribute));
			serviceRequest._URL = mMessageV2URL + "PostNewMessageToGroupListWithRMF";
			ServiceCall<RMFMessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
		else
		{
			serviceRequest._URL = mMessageURL + "PostNewGroupMessageToList";
			ServiceCall<MessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
	}

	public static void PostMessageReply(MessageRequest messagerequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = null;
		serviceRequest = ((messagerequest.level != MessageLevel.WhiteList) ? new ServiceRequest
		{
			_Type = WsServiceType.POST_REPLY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		} : new ServiceRequest
		{
			_Type = WsServiceType.POST_REPLY_RMF,
			_EventDelegate = inCallback,
			_UserData = inUserData
		});
		AddCommon(serviceRequest);
		string text = UtUtilities.SerializeToXml(messagerequest);
		serviceRequest.AddParam("messagerequest", text);
		if (messagerequest.level == MessageLevel.WhiteList)
		{
			string text2 = Ticks.ToString();
			serviceRequest.AddParam("ticks", text2);
			serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
			serviceRequest._URL = mMessageV3URL + "PostMessageReplyWithRMF";
			ServiceCall<RMFMessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
		else
		{
			serviceRequest._URL = mMessageURL + "PostMessageReply";
			ServiceCall<MessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
	}

	public static void PostGroupMessageReply(string groupID, string message, MessageLevel level, int replyTo, string displayAttribute, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = null;
		serviceRequest = ((level != MessageLevel.WhiteList) ? new ServiceRequest
		{
			_Type = WsServiceType.POST_GROUP_MESSAGE_REPLY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		} : new ServiceRequest
		{
			_Type = WsServiceType.POST_GROUP_MESSAGE_REPLY_RMF,
			_EventDelegate = inCallback,
			_UserData = inUserData
		});
		AddCommon(serviceRequest);
		serviceRequest.AddParam("groupID", groupID);
		serviceRequest.AddParam("content", message);
		serviceRequest.AddParam("level", level.ToString());
		serviceRequest.AddParam("replyTo", replyTo.ToString());
		serviceRequest.AddParam("displayAttribute", displayAttribute);
		if (level == MessageLevel.WhiteList)
		{
			string text = Ticks.ToString();
			serviceRequest.AddParam("ticks", text);
			serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + groupID + message + level.ToString() + replyTo + displayAttribute));
			serviceRequest._URL = mMessageV2URL + "PostGroupMessageReplyWithRMF";
			ServiceCall<RMFMessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
		else
		{
			serviceRequest._URL = mMessageURL + "PostGroupMessageReply";
			ServiceCall<MessageBoardPostResult>.Create(serviceRequest)?.DoSet();
		}
	}

	public static void GetItemsInStoreData(int storeId, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ITEMS_IN_STORE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("storeId", storeId);
		obj._URL = mItemStoreURL + "GetItemsInStore";
		ServiceCall<ItemsInStoreData>.Create(obj)?.DoGet();
	}

	public static void GetStore(int[] storeIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_STORE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(new GetStoreRequest
		{
			StoreIDs = storeIDs
		});
		obj.AddParam("getStoreRequest", inVal);
		obj._URL = mItemStoreURL + "GetStore";
		ServiceCall<GetStoreResponse>.Create(obj)?.DoGet();
	}

	public static void IsValidToken(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.IS_VALID_TOKEN,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAuthenticationURL + "IsValidApiToken";
		ServiceCall<bool>.Create(obj)?.DoGet();
	}

	public static void IsValidApiToken(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.IS_VALID_API_TOKEN,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mAuthenticationURL + "IsValidApiToken_V2";
		ServiceCall<ApiTokenStatus>.Create(obj)?.DoGet();
	}

	public static void ProcessSteam(SteamPurchaseRequest inRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.PROCESS_STEAM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = TripleDES.EncryptUnicode(UtUtilities.SerializeToXml(inRequest), mSecret);
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mPaymentURL + "ProcessSteam";
		ServiceCall<ReceiptRedemptionResult>.Create(obj)?.DoSet();
	}

	public static void RedeemReceipt(ReceiptRedemptionRequest inRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REDEEM_RECEIPT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(inRequest);
		Debug.LogError("Reciept Data IAP Item SnoggleTog Test : " + text);
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mPaymentURL + "RedeemReceipt";
		ServiceCall<ReceiptRedemptionResult>.Create(obj)?.DoSet();
	}

	public static void GetXsollaToken(AccessTokenRequest purchaseInfo, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_XSOLLA_TOKEN;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("apiToken", ProductConfig.pToken);
		string text = TripleDES.EncryptUnicode(UtUtilities.SerializeToXml(purchaseInfo), mSecret);
		serviceRequest.AddParam("request", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + ProductConfig.pToken + text));
		serviceRequest._URL = mPaymentV2URL + "GetToken";
		ServiceCall<GetAccessTokenResponse>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoSet();
	}

	public static void CreatePurchaseOrderRequest(string requestData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_PURCHASE_ORDER_REQUEST;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("apiToken", ProductConfig.pToken);
		string text = TripleDES.EncryptUnicode(requestData, mSecret);
		serviceRequest.AddParam("request", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + ProductConfig.pToken + text));
		serviceRequest._URL = mPaymentV2URL + "CreatePurchaseOrder";
		ServiceCall<GetCreateOrderResponse>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoGet();
	}

	public static void SetUserChestFound(string sceneName, string groupName, string chestName, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_CHEST_FOUND,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("sceneName", sceneName);
		serviceRequest.AddParam("groupName", groupName);
		serviceRequest.AddParam("chestName", chestName);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + sceneName + groupName + chestName));
		serviceRequest._URL = mContentURL + "SetUserChestFound";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void SetHighScore(int score, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_HIGH_SCORE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userScore", score);
		obj._URL = mContentURL + "SetHighScore";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetDisplayNames(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_DISPLAY_NAMES,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetDisplayNames";
		ServiceCall<DisplayNameList>.Create(obj)?.DoGet();
	}

	public static void SetDisplayName(SetDisplayNameRequest requestObj, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_DISPLAY_NAME,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(requestObj);
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "SetDisplayName";
		ServiceCall<SetAvatarResult>.Create(obj)?.DoSet();
	}

	public static void GetDisplayNamesByCategory(int categoryID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_DISPLAY_NAMES_BY_CATEGORY_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("categoryID", categoryID);
		obj._URL = mContentURL + "GetDisplayNamesByCategoryID";
		ServiceCall<DisplayNameList>.Create(obj)?.DoGet();
	}

	public static void GetPayout(string moduleName, int points, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_PAYOUT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("ModuleName", moduleName);
		obj.AddParam("points", points);
		obj._URL = mMissionURL + "GetPayout";
		ServiceCall<int>.Create(obj)?.DoGet();
	}

	public static void GetGameCurrency(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAME_CURRENCY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetGameCurrency";
		ServiceCall<int>.Create(obj)?.DoGet();
	}

	public static void GetGameCurrencyByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAME_CURRENCY_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetGameCurrencyByUserID";
		ServiceCall<int>.Create(obj)?.DoGet();
	}

	public static void GetUserGameCurrency(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_GAME_CURRENCY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetUserGameCurrency";
		ServiceCall<UserGameCurrency>.Create(obj)?.DoGet();
	}

	public static void SetGameCurrency(int amount, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_GAME_CURRENCY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("amount", amount);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + amount));
		obj._URL = mContentURL + "SetGameCurrency";
		ServiceCall<int>.Create(obj)?.DoSet();
	}

	public static void CollectUserBonus(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_BONUS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken));
		obj._URL = mContentURL + "CollectUserBonus";
		ServiceCall<ArrayOfUserBonus>.Create(obj)?.DoGet();
	}

	public static void GetItemData(int itemId, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ITEM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("itemId", itemId);
		obj._URL = mItemStoreURL + "GetItem";
		ServiceCall<ItemData>.Create(obj)?.DoGet();
	}

	public static void GetItems(int productGroupID, List<int> itemIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(new GetItemRequest
		{
			ProductGroupID = productGroupID,
			ItemIDs = itemIDs
		});
		obj.AddParam("itemRequests", inVal);
		obj._URL = mItemStoreURL + "GetItems";
		ServiceCall<ArrayOfItemData>.Create(obj)?.DoGet();
	}

	public static void GetTreasureChest(string sceneName, string groupName, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TREASURE_CHEST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("sceneName", sceneName);
		obj.AddParam("groupName", groupName);
		obj._URL = mMissionURL + "GetTreasureChest";
		ServiceCall<TreasureChestData>.Create(obj)?.DoGet();
	}

	public static void GetUserMissionState(string userid, MissionRequestFilter filter, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_MISSION_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userid);
		string inVal = UtUtilities.ProcessSendObject(filter);
		obj.AddParam("filter", inVal);
		obj._URL = mContentURL + "GetUserMissionState";
		ServiceCall<UserMissionStateResult>.Create(obj)?.DoGet();
	}

	public static void GetUserMissionStateV2(string userid, MissionRequestFilterV2 filter, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_MISSION_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userid);
		string inVal = UtUtilities.ProcessSendObject(filter);
		obj.AddParam("filter", inVal);
		obj._URL = mContentV2URL + "GetUserMissionState";
		ServiceCall<UserMissionStateResult>.Create(obj)?.DoGet();
	}

	public static void GetUserUpcomingMissionState(string userid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_UPCOMING_MISSION_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userid);
		obj._URL = mContentV2URL + "GetUserUpcomingMissionState";
		ServiceCall<UserMissionStateResult>.Create(obj)?.DoGet();
	}

	public static void GetUserActiveMissionState(string userid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ACTIVE_MISSION_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userid);
		obj._URL = mContentV2URL + "GetUserActiveMissionState";
		ServiceCall<UserMissionStateResult>.Create(obj)?.DoGet();
	}

	public static void GetUserCompletedMissionState(string userid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_COMPLETED_MISSION_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userid);
		obj._URL = mContentV2URL + "GetUserCompletedMissionState";
		ServiceCall<UserMissionStateResult>.Create(obj)?.DoGet();
	}

	public static void GetUserTimedMissionState(string userid, MissionRequestFilterV2 filter, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_TIMED_MISSION_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userid);
		string inVal = UtUtilities.ProcessSendObject(filter);
		obj.AddParam("filter", inVal);
		obj._URL = mContentV2URL + "GetUserTimedMissionState";
		ServiceCall<UserMissionStateResult>.Create(obj)?.DoGet();
	}

	public static void SetTaskState(string userID, int missionID, int taskID, bool completed, string payload, int containerid, CommonInventoryRequest[] inventoryRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_TASK_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("missionId", missionID);
		serviceRequest.AddParam("taskId", taskID);
		serviceRequest.AddParam("completed", completed.ToString());
		serviceRequest.AddParam("xmlPayload", payload);
		serviceRequest.AddParam("ContainerId", containerid);
		string text = UtUtilities.ProcessSendObject(inventoryRequest);
		serviceRequest.AddParam("commonInventoryRequestXml", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + userID + missionID + taskID + completed + payload + containerid + text));
		serviceRequest._URL = mContentV2URL + "SetTaskState";
		ServiceCall<SetTaskStateResult>.Create(serviceRequest)?.DoSet();
	}

	public static void SetTimedMissionTaskState(string userID, int missionID, int taskID, bool completed, string payload, int containerid, CommonInventoryRequest[] inventoryRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_TIMED_MISSION_TASK_STATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(new TaskStateRequest
		{
			UserID = new Guid(userID),
			MissionID = missionID,
			TaskID = taskID,
			Completed = completed,
			Payload = payload,
			ContainerID = containerid,
			CommonInventoryRequestItems = inventoryRequest
		});
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetTimedMissionTaskState";
		ServiceCall<SetTimedMissionTaskStateResult>.Create(obj)?.DoSet();
	}

	public static void AcceptMission(string userID, int missionID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.ACCEPT_MISSION,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("missionId", missionID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + missionID));
		serviceRequest._URL = mContentURL + "AcceptMission";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void AcceptTimedMission(string userID, int missionID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.ACCEPT_TIMED_MISSION,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("missionId", missionID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + missionID));
		serviceRequest._URL = mContentURL + "AcceptTimedMission";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetUserTreasureChest(string sceneName, string groupName, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_TREASURE_CHEST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("sceneName", sceneName);
		obj.AddParam("groupName", groupName);
		obj._URL = mContentURL + "GetUserTreasureChest";
		ServiceCall<UserTreasureChestData>.Create(obj)?.DoGet();
	}

	public static void SetUserTreasureChest(string sceneName, string groupName, UserTreasureChestData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_TREASURE_CHEST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("sceneName", sceneName);
		serviceRequest.AddParam("groupName", groupName);
		string text = UtUtilities.ProcessSendObject(inData);
		serviceRequest.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + sceneName + groupName + text));
		serviceRequest._URL = mContentURL + "SetUserTreasureChest";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void SetGameData(string userID, int gameID, bool isMultiplayer, int difficulty, int level, string xmlData, bool win, bool loss, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_GAME_DATA,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("gameId", gameID);
		serviceRequest.AddParam("isMultiplayer", isMultiplayer.ToString());
		serviceRequest.AddParam("difficulty", difficulty);
		serviceRequest.AddParam("gameLevel", level);
		serviceRequest.AddParam("xmlDocumentData", xmlData);
		serviceRequest.AddParam("win", win.ToString());
		serviceRequest.AddParam("loss", loss.ToString());
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + gameID + isMultiplayer + difficulty + level + xmlData + win + loss));
		serviceRequest._URL = mContentURL + "SendRawGameData";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetGameDataByUser(string userID, int gameID, bool isMultiplayer, int difficulty, int level, string key, int count, bool ascendingOrder, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAME_DATA_USER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("gameId", gameID);
		obj.AddParam("isMultiplayer", isMultiplayer.ToString());
		obj.AddParam("difficulty", difficulty);
		obj.AddParam("gameLevel", level);
		obj.AddParam("key", key);
		obj.AddParam("count", count);
		obj.AddParam("AscendingOrder", ascendingOrder.ToString());
		obj._URL = mContentURL + "GetGameDataByUser";
		ServiceCall<GameDataSummary>.Create(obj)?.DoGet();
	}

	public static void GetGameDataByGame(string userID, int gameID, bool isMultiplayer, int difficulty, int level, string key, int count, bool ascendingOrder, int? score, bool buddyFilter, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAME_DATA_GAME,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("gameId", gameID);
		serviceRequest.AddParam("isMultiplayer", isMultiplayer.ToString());
		serviceRequest.AddParam("difficulty", difficulty);
		serviceRequest.AddParam("gameLevel", level);
		serviceRequest.AddParam("key", key);
		serviceRequest.AddParam("count", count);
		serviceRequest.AddParam("AscendingOrder", ascendingOrder.ToString());
		if (score.HasValue)
		{
			serviceRequest.AddParam("score", score.Value);
		}
		else
		{
			serviceRequest.AddParam("score", -1);
		}
		serviceRequest.AddParam("buddyFilter", buddyFilter.ToString());
		serviceRequest._URL = mContentURL + "GetGameDataByGame";
		ServiceCall<GameDataSummary>.Create(serviceRequest)?.DoGet();
	}

	public static void GetGameDataByGameForDayRange(string userID, int gameID, bool isMultiplayer, int difficulty, int level, string key, int count, bool ascendingOrder, int? score, bool buddyFilter, DateTime startDate, DateTime endDate, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAME_DATA_GAME_DAY_RANGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("gameId", gameID);
		serviceRequest.AddParam("isMultiplayer", isMultiplayer.ToString());
		serviceRequest.AddParam("difficulty", difficulty);
		serviceRequest.AddParam("gameLevel", level);
		serviceRequest.AddParam("key", key);
		serviceRequest.AddParam("count", count);
		serviceRequest.AddParam("AscendingOrder", ascendingOrder.ToString());
		if (score.HasValue)
		{
			serviceRequest.AddParam("score", score.Value);
		}
		else
		{
			serviceRequest.AddParam("score", -1);
		}
		serviceRequest.AddParam("startDate", startDate.ToString(UtUtilities.GetCultureInfo("en-US")));
		serviceRequest.AddParam("endDate", endDate.ToString(UtUtilities.GetCultureInfo("en-US")));
		serviceRequest.AddParam("buddyFilter", buddyFilter.ToString());
		serviceRequest._URL = mContentV2URL + "GetGameDataByGameForDateRange";
		ServiceCall<GameDataSummary>.Create(serviceRequest)?.DoGet();
	}

	public static void GetCumulativePeriodicGameDataByUsers(Guid[] userIDs, int gameID, bool isMultiplayer, int difficulty, int level, string key, bool ascendingOrder, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_CUMULATIVE_GAME_DATA_USERS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(userIDs);
		obj.AddParam("userIds", inVal);
		obj.AddParam("gameId", gameID);
		obj.AddParam("isMultiplayer", isMultiplayer.ToString());
		obj.AddParam("difficulty", difficulty);
		obj.AddParam("gameLevel", level);
		obj.AddParam("key", key);
		obj.AddParam("AscendingOrder", ascendingOrder.ToString());
		obj._URL = mContentURL + "GetCumulativePeriodicGameDataByUsers";
		ServiceCall<GameDataSummary>.Create(obj)?.DoGet();
	}

	public static void GetPeriodicGameDataByGame(string userID, int gameID, bool isMultiplayer, int difficulty, int level, string key, int count, bool ascendingOrder, int? score, bool buddyFilter, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.GET_PERIODIC_GAME_DATA_GAME,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("gameId", gameID);
		serviceRequest.AddParam("isMultiplayer", isMultiplayer.ToString());
		serviceRequest.AddParam("difficulty", difficulty);
		serviceRequest.AddParam("gameLevel", level);
		serviceRequest.AddParam("key", key);
		serviceRequest.AddParam("count", count);
		serviceRequest.AddParam("AscendingOrder", ascendingOrder.ToString());
		if (score.HasValue)
		{
			serviceRequest.AddParam("score", score.Value);
		}
		else
		{
			serviceRequest.AddParam("score", -1);
		}
		serviceRequest.AddParam("buddyFilter", buddyFilter.ToString());
		serviceRequest._URL = mContentURL + "GetPeriodicGameDataByGame";
		ServiceCall<GameDataSummary>.Create(serviceRequest)?.DoGet();
	}

	public static void GetGameData(GetGameDataRequest dataRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAME_DATA,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.SerializeToXml(dataRequest);
		obj.AddParam("gameDataRequest", inVal);
		obj._URL = mContentV2URL + "GetGameData";
		ServiceCall<GetGameDataResponse>.Create(obj)?.DoGet();
	}

	public static void GetGameDataByGroup(string userID, string groupID, int gameID, bool isMultiplayer, int difficulty, int level, string key, int count, bool ascendingOrder, int? score, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAME_DATA_GROUP,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("groupID", groupID);
		serviceRequest.AddParam("gameId", gameID);
		serviceRequest.AddParam("isMultiplayer", isMultiplayer.ToString());
		serviceRequest.AddParam("difficulty", difficulty);
		serviceRequest.AddParam("gameLevel", level);
		serviceRequest.AddParam("key", key);
		serviceRequest.AddParam("count", count);
		serviceRequest.AddParam("AscendingOrder", ascendingOrder.ToString());
		if (score.HasValue)
		{
			serviceRequest.AddParam("score", score.Value);
		}
		else
		{
			serviceRequest.AddParam("score", -1);
		}
		serviceRequest._URL = mContentURL + "GetGameDataByGroup";
		ServiceCall<GameDataSummary>.Create(serviceRequest)?.DoGet();
	}

	public static void SetKeyValuePair(int pairID, PairData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_KEY_VALUE_PAIR,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("pairId", pairID);
		string text = UtUtilities.ProcessSendObject(inData);
		serviceRequest.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + pairID + text));
		serviceRequest._URL = mContentURL + "SetKeyValuePair";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void SetKeyValuePairByUserID(string userID, int pairID, PairData inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_KEY_VALUE_PAIR_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("pairId", pairID);
		string text = UtUtilities.ProcessSendObject(inData);
		serviceRequest.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + userID + pairID + text));
		serviceRequest._URL = mContentURL + "SetKeyValuePairByUserID";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetKeyValuePair(int pairID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_KEY_VALUE_PAIR,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("pairId", pairID);
		obj._URL = mContentURL + "GetKeyValuePair";
		ServiceCall<PairData>.Create(obj)?.DoGet();
	}

	public static void GetKeyValuePairByUserID(string userID, int pairID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_KEY_VALUE_PAIR_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("pairId", pairID);
		obj._URL = mContentURL + "GetKeyValuePairByUserID";
		ServiceCall<PairData>.Create(obj)?.DoGet();
	}

	public static void DeleteKeyValuePair(int pairID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.DELETE_KEY_VALUE_PAIR,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("pairId", pairID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + pairID));
		obj._URL = mContentURL + "DelKeyValuePair";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void DeleteKeyValuePairByKey(int pairID, string inKey, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.DELETE_KEY_VALUE_PAIR_BY_KEY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("pairId", pairID);
		serviceRequest.AddParam("pairKey", inKey);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + pairID + inKey));
		serviceRequest._URL = mContentURL + "DelKeyValuePairByKey";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void DeleteKeyValuePairByKeys(string userID, int pairID, string[] inKeys, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.DELETE_KEY_VALUE_PAIR_BY_KEYS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("pairId", pairID);
		string text = UtUtilities.ProcessSendObject(inKeys);
		serviceRequest.AddParam("pairKeys", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + userID + pairID + text));
		serviceRequest._URL = mContentURL + "DelKeyValuePairByKeys";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetAssetVersion(string assetname, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_ASSET_VERSION;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("assetName", assetname);
		serviceRequest._URL = mItemStoreURL + "GetAssetVersionByAssetName";
		ServiceCall<AssetVersion>.Create(serviceRequest)?.DoGet();
	}

	public static void GetAllPlatformAssetVersions(int inPlatformID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_ALL_PLATFORM_ASSET_VERSIONS;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", ProductConfig.pApiKey);
		serviceRequest.AddParam("platformID", inPlatformID);
		serviceRequest._URL = mItemStoreURL + "GetAllPlatformAssetVersions";
		ServiceCall<AssetVersion[]>.Create(serviceRequest)?.DoGet();
	}

	public static void GetAssetVersions(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_ASSET_VERSIONS;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		AssetRequest obj = new AssetRequest
		{
			ProductID = ProductConfig.pProductID,
			PlatformID = 1,
			Locale = UtUtilities.GetLocaleLanguage(),
			AssetName = "",
			AppVersion = ProductConfig.pProductVersion
		};
		serviceRequest.AddParam("apiKey", ProductConfig.pApiKey);
		serviceRequest.AddParam("request", UtUtilities.SerializeToString(obj));
		serviceRequest._URL = mItemStoreURL + "GetAssetVersions";
		ServiceCall<AssetVersion[]>.Create(serviceRequest)?.DoGet();
	}

	public static void GetAllAssetVersionsByUser(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_ASSET_VERSIONS;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", ProductConfig.pApiKey);
		serviceRequest.AddParam("apiToken", ProductConfig.pToken);
		serviceRequest._URL = mItemStoreURL + "GetAllAssetVersionsByUser";
		ServiceCall<AssetVersion[]>.Create(serviceRequest)?.DoGet();
	}

	public static void SetNickname(string userID, string otherUserID, string nickname, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_NICKNAME,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("otherId", otherUserID);
		serviceRequest.AddParam("nickName", nickname);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + otherUserID + nickname));
		serviceRequest._URL = mContentURL + "SetNickname";
		ServiceCall<NicknameSetResult>.Create(serviceRequest)?.DoGet();
	}

	public static void GetNickname(string userID, string buddyID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_NICKNAME,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("otherId", buddyID);
		obj._URL = mContentURL + "GetNickname";
		ServiceCall<Nickname>.Create(obj)?.DoGet();
	}

	public static void GetNicknames(string userID, string[] otherIDList, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_NICKNAMES,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		string inVal = "";
		if (otherIDList != null)
		{
			inVal = UtUtilities.ProcessSendObject(otherIDList);
		}
		obj.AddParam("otherIds", inVal);
		obj._URL = mContentURL + "GetNicknames";
		ServiceCall<Nickname[]>.Create(obj)?.DoGet();
	}

	public static void GetBuddyList(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_BUDDY_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetBuddyList";
		ServiceCall<BuddyList>.Create(obj)?.DoGet();
	}

	public static void GetFriendCode(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_FRIEND_CODE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetFriendCode";
		ServiceCall<string>.Create(obj)?.DoGet();
	}

	public static void AddBuddy(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.ADD_BUDDY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("buddyUserID", userID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID));
		obj._URL = mContentURL + "AddBuddy";
		ServiceCall<BuddyActionResult>.Create(obj)?.DoSet();
	}

	public static void AddBuddyByFriendCode(string code, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.ADD_BUDDY_BY_FRIEND_CODE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("friendCode", code);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + code));
		obj._URL = mContentURL + "AddBuddyByFriendCode";
		ServiceCall<BuddyActionResult>.Create(obj)?.DoSet();
	}

	public static void AddOneWayBuddy(Guid[] userIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.ADD_ONE_WAY_BUDDY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(userIDs);
		obj.AddParam("buddyUserIDs", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "AddOneWayBuddy";
		ServiceCall<ArrayOfBuddyActionResult>.Create(obj)?.DoGet();
	}

	public static void ApproveBuddy(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.APPROVE_BUDDY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("buddyUserID", userID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID));
		obj._URL = mContentURL + "ApproveBuddy";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void BlockBuddy(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.BLOCK_BUDDY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("buddyUserID", userID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID));
		obj._URL = mContentURL + "BlockBuddy";
		ServiceCall<bool>.Create(obj)?.DoGet();
	}

	public static void GetBuddyLocation(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_BUDDY_LOCATION,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("buddyUserID", userID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID));
		obj._URL = mContentURL + "GetBuddyLocation";
		ServiceCall<BuddyLocation>.Create(obj)?.DoGet();
	}

	public static void InviteBuddy(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.INVITE_BUDDY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("buddyUserID", userID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID));
		obj._URL = mContentURL + "InviteBuddy";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void RemoveBuddy(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REMOVE_BUDDY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("buddyUserID", userID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID));
		obj._URL = mContentURL + "RemoveBuddy";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void UpdateBestBuddy(string userID, bool bestBuddy, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.UPDATE_BEST_BUDDY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("buddyUserID", userID);
		serviceRequest.AddParam("bestBuddy", bestBuddy.ToString());
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + bestBuddy));
		serviceRequest._URL = mContentURL + "UpdateBestBuddy";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetNeighborsByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_NEIGHBORS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetNeighborsByUserID";
		ServiceCall<NeighborData>.Create(obj)?.DoGet();
	}

	public static void SetNeighbor(string userID, string neighborUserID, int inslot, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_NEIGHBOR,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("neighboruserid", neighborUserID);
		serviceRequest.AddParam("slot", inslot);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + neighborUserID + inslot));
		serviceRequest._URL = mContentURL + "SetNeighbor";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void GetPartyByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_PARTY_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetPartyByUserID";
		ServiceCall<UserPartyComplete>.Create(obj)?.DoGet();
	}

	public static void GetPartiesByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_PARTIES_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetPartiesByUserID";
		ServiceCall<ArrayOfUserPartyComplete>.Create(obj)?.DoGet();
	}

	public static void GetActiveParties(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACTIVE_PARTIES,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetActiveParties";
		ServiceCall<UserPartyData>.Create(obj)?.DoGet();
	}

	public static void PurchaseParty(int itemID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.PURCHASE_PARTY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("itemId", itemID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + itemID));
		obj._URL = mContentURL + "PurchaseParty";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetUserItemPositions(string userID, string roomID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ITEM_POSITION_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("roomID", roomID);
		obj._URL = mContentURL + "GetUserRoomItemPositions";
		ServiceCall<UserItemPositionList>.Create(obj)?.DoGet();
	}

	public static void SetUserItemPositions(string roomID, UserItemPositionSetRequest[] inCreateList, UserItemPositionSetRequest[] inUpdateList, int[] inRemoveList, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_ITEM_POSITION_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(inCreateList);
		string text2 = UtUtilities.ProcessSendObject(inUpdateList);
		string text3 = UtUtilities.ProcessSendObject(inRemoveList);
		serviceRequest.AddParam("createXml", text);
		serviceRequest.AddParam("updateXml", text2);
		serviceRequest.AddParam("removeXml", text3);
		serviceRequest.AddParam("roomID", roomID);
		string text4 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text4);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text4 + mSecret + mUserToken + text + text2 + text3 + roomID));
		serviceRequest._URL = mContentURL + "SetUserRoomItemPositions";
		ServiceCall<UserItemPositionSetResponse>.Create(serviceRequest)?.DoSet();
	}

	public static void GetUserTimedItems(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_TIMED_ITEM_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetUserTimedItemByUserID";
		ServiceCall<ArrayOfUserTimedItem>.Create(obj)?.DoGet();
	}

	public static void SetUserTimedItem(UserTimedItem item, UserTimedItemFlag flag, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_TIMED_ITEM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(item);
		serviceRequest.AddParam("inputXml", text);
		serviceRequest.AddParam("flag", (int)flag);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		string[] obj = new string[5] { text2, mSecret, mUserToken, text, null };
		int num = (int)flag;
		obj[4] = num.ToString();
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(string.Concat(obj)));
		serviceRequest._URL = mContentURL + "SetUserTimedItemFlagged";
		ServiceCall<UserTimedItem>.Create(serviceRequest)?.DoSet();
	}

	public static void DeleteUserTimedItem(int userTimedItemID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.DELETE_USER_TIMED_ITEM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userTimedItemID", userTimedItemID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userTimedItemID));
		obj._URL = mContentURL + "DeleteUserTimedItem";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void GetUserStaff(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_STAFF_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetUserStaffByUserID";
		ServiceCall<ArrayOfUserStaff>.Create(obj)?.DoGet();
	}

	public static void SetUserStaff(UserStaff staff, UserStaffFlag flag, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_STAFF,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(staff);
		serviceRequest.AddParam("inputXml", text);
		serviceRequest.AddParam("flag", (int)flag);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		string[] obj = new string[5] { text2, mSecret, mUserToken, text, null };
		int num = (int)flag;
		obj[4] = num.ToString();
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(string.Concat(obj)));
		serviceRequest._URL = mContentURL + "SetUserStaffFlagged";
		ServiceCall<UserStaff>.Create(serviceRequest)?.DoSet();
	}

	public static void GetAuthoritativeTime(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_AUTHORITATIVE_TIME,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mContentURL + "GetAuthoritativeTime";
		ServiceCall<DateTime>.Create(obj)?.DoGet();
	}

	public static void GetStreamPost(int streamPostID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_STREAM_POST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("streamPostID", streamPostID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + streamPostID));
		obj._URL = mFacebookURL + "GetStreamPost";
		ServiceCall<StreamPost>.Create(obj)?.DoGet();
	}

	public static void StreamPostLog(StreamPostLog log, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_STREAM_POST_LOG,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(log);
		obj.AddParam("inputXml", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mFacebookURL + "SetStreamPostLog";
		ServiceCall<KA.Framework.StreamPostLog>.Create(obj)?.DoSet();
	}

	public static void GetAllActivePetsByuserId(string userID, bool active, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ALL_ACTIVE_PETS_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("active", active.ToString());
		obj._URL = mContentV2URL + "GetAllActivePetsByuserId";
		ServiceCall<RaisedPetData[]>.Create(obj)?.DoGet();
	}

	public static void GetActivePet(string userID, int petTypeID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACTIVE_RAISED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("petTypeID", petTypeID);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetActiveRaisedPet";
		ServiceCall<RaisedPetData[]>.Create(obj)?.DoGet();
	}

	public static void GetActiveRaisedPetsByTypes(string userID, int[] petTypeIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACTIVE_RAISED_PETS_BY_TYPEIDS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = "";
		if (petTypeIDs.Length != 0)
		{
			text = petTypeIDs[0].ToString();
			for (int i = 1; i < petTypeIDs.Length; i++)
			{
				text = text + "," + petTypeIDs[i];
			}
		}
		serviceRequest.AddParam("petTypeIDs", text);
		serviceRequest.AddParam("userId", userID);
		serviceRequest._URL = mContentURL + "GetActiveRaisedPetsByTypes";
		ServiceCall<RaisedPetData[]>.Create(serviceRequest)?.DoGet();
	}

	public static void GetReleasedPet(string userID, int petTypeID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_RELEASED_RAISED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("petTypeID", petTypeID);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetInactiveRaisedPet";
		ServiceCall<RaisedPetData[]>.Create(obj)?.DoGet();
	}

	public static void GetSelectedRaisedPet(string userID, bool selected, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_SELECTED_RAISED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("isActive", selected.ToString());
		obj._URL = mContentURL + "GetSelectedRaisedPet";
		ServiceCall<RaisedPetData[]>.Create(obj)?.DoGet();
	}

	public static void GetSelectedRaisedPetByType(string userID, int petTypeID, bool selected, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_SELECTED_RAISED_PET_BY_TYPE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("petTypeID", petTypeID);
		obj.AddParam("isActive", selected.ToString());
		obj._URL = mContentURL + "GetSelectedPetByType";
		ServiceCall<RaisedPetData[]>.Create(obj)?.DoGet();
	}

	public static void GetUnselectedPetsByTypes(string userID, int[] petTypeIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.GET_UNSELECTED_PETS_BY_TYPES,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = "";
		if (petTypeIDs.Length != 0)
		{
			text = petTypeIDs[0].ToString();
			for (int i = 1; i < petTypeIDs.Length; i++)
			{
				text = text + "," + petTypeIDs[i];
			}
		}
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("petTypeIDs", text);
		serviceRequest._URL = mContentURL + "GetUnselectedPetByTypes";
		ServiceCall<RaisedPetData[]>.Create(serviceRequest)?.DoGet();
	}

	public static void CreateRaisedPet(int petTypeID, bool selectPet, bool unselectOtherPets, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.CREATE_RAISED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("request", UtUtilities.SerializeToString(new RaisedPetRequest
		{
			PetTypeID = petTypeID,
			UserID = null,
			SetAsSelectedPet = selectPet,
			UnSelectOtherPets = unselectOtherPets
		}));
		obj._URL = mContentV2URL + "CreateRaisedPet";
		ServiceCall<RaisedPetData>.Create(obj)?.DoSet();
	}

	public static void CreatePet(int petTypeID, bool selectPet, bool unselectOtherPets, RaisedPetData inData, CommonInventoryRequest[] inInventoryRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.CREATE_RAISED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(new RaisedPetRequest
		{
			PetTypeID = petTypeID,
			UserID = null,
			SetAsSelectedPet = selectPet,
			UnSelectOtherPets = unselectOtherPets,
			RaisedPetData = inData,
			ProductGroupID = ProductConfig.pProductGroupID,
			CommonInventoryRequests = inInventoryRequest
		});
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "CreatePet";
		ServiceCall<CreatePetResponse>.Create(obj)?.DoSet();
	}

	public static void SetRaisedPet(RaisedPetData inData, CommonInventoryRequest[] inInventoryRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_ACTIVE_RAISED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(new RaisedPetRequest
		{
			RaisedPetData = inData,
			CommonInventoryRequests = inInventoryRequest
		});
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV3URL + "SetRaisedPet";
		ServiceCall<SetRaisedPetResponse>.Create(obj)?.DoSet();
	}

	public static void SetRaisedPetInactive(int petID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_RAISED_PET_INACTIVE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("raisedPetID", petID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + petID));
		obj._URL = mContentURL + "SetRaisedPetInactive";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void SetSelectedPet(int raisedPetID, bool unselectOtherPets, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_SELECTED_PET,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("raisedPetID", raisedPetID);
		serviceRequest.AddParam("unselectOtherPets", unselectOtherPets.ToString());
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + raisedPetID + unselectOtherPets));
		serviceRequest._URL = mContentURL + "SetSelectedPet";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetUserActivity(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ACTIVITY_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mContentURL + "GetUserActivityByUserID";
		ServiceCall<ArrayOfUserActivity>.Create(obj)?.DoGet();
	}

	public static void SetUserActivity(UserActivity activity, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_ACTIVITY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(activity);
		obj.AddParam("inputXml", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentURL + "SetUserActivity";
		ServiceCall<UserActivity>.Create(obj)?.DoSet();
	}

	public static void SubmitRating(int categoryID, int entityID, int ratedValue, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SUBMIT_RATING,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("categoryID", categoryID);
		serviceRequest.AddParam("ratedEntityID", entityID);
		serviceRequest.AddParam("ratedValue", ratedValue);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + categoryID + entityID + ratedValue));
		serviceRequest._URL = mRatingURL + "SetRating";
		ServiceCall<RatingInfo>.Create(serviceRequest)?.DoSet();
	}

	public static void SubmitRatingForUserID(int categoryID, string roomID, string userID, string ratedUserID, int ratedValue, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SUBMIT_RATING_FOR_USERID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.SerializeToXml(new UserRatingRequest
		{
			ProductGroupID = ProductConfig.pProductGroupID,
			CategoryID = categoryID,
			RoomID = roomID,
			UserID = new Guid(userID),
			RatedUserID = new Guid(ratedUserID),
			RatedValue = ratedValue
		});
		obj.AddParam("request", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mRatingV2URL + "SetUserRating";
		ServiceCall<RatingInfo>.Create(obj)?.DoSet();
	}

	public static void GetAllRatings(int categoryID, int entityID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ALL_RATINGS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("categoryID", categoryID);
		obj.AddParam("ratedEntityID", entityID);
		obj._URL = mRatingURL + "GetRatingByRatedEntity";
		ServiceCall<RatingInfo[]>.Create(obj)?.DoGet();
	}

	public static void ClearRating(int categoryID, int entityID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.CLEAR_RATING,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("categoryID", categoryID);
		serviceRequest.AddParam("ratedEntityID", entityID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + categoryID + entityID));
		serviceRequest._URL = mRatingURL + "DeleteEntityRating";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetTopRank(int categoryID, int numRecords, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_RANK,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("categoryID", categoryID);
		obj.AddParam("numberOfRecord", numRecords);
		obj._URL = mRatingURL + "GetTopRatedByCategoryID";
		ServiceCall<RatingRankInfo[]>.Create(obj)?.DoGet();
	}

	public static void GetTopRanksWithUserIDs(string userID, int categoryID, int numRecords, RequestType typeID, List<long> facebookFriends, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_RANKS_WITH_USERID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.SerializeToXml(new UserRatingRankRequest
		{
			UserID = new Guid(userID),
			CategoryID = categoryID,
			Type = typeID,
			Count = numRecords,
			FacebookUserIDs = facebookFriends
		});
		obj.AddParam("request", inVal);
		obj._URL = mRatingV2URL + "GetTopRatedUserByCategoryID";
		ServiceCall<ArrayOfUserRatingRankInfo>.Create(obj)?.DoGet();
	}

	public static void GetRank(int categoryID, int entityID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_RANK,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("categoryID", categoryID);
		obj.AddParam("ratedEntityID", entityID);
		obj._URL = mRatingURL + "GetEntityRatedRank";
		ServiceCall<RatingRankInfo>.Create(obj)?.DoGet();
	}

	public static void GetRatingForRatedUser(int categoryID, string roomID, string ratedUserID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_RATING_FOR_RATED_USERS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.SerializeToXml(new UserRatingRequest
		{
			ProductGroupID = ProductConfig.pProductGroupID,
			CategoryID = categoryID,
			RoomID = "",
			RatedUserID = new Guid(ratedUserID)
		});
		obj.AddParam("request", inVal);
		obj._URL = mRatingV2URL + "GetRatingForRatedUser";
		ServiceCall<int>.Create(obj)?.DoGet();
	}

	public static void GetAverageRatingForRoom(int categoryID, string roomID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_AVERAGE_RATING_FOR_ROOM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.SerializeToXml(new UserRatingRequest
		{
			ProductGroupID = ProductConfig.pProductGroupID,
			CategoryID = categoryID,
			RoomID = ""
		});
		obj.AddParam("request", inVal);
		obj._URL = mRatingV2URL + "GetAverageRatingForRoom";
		ServiceCall<int>.Create(obj)?.DoGet();
	}

	public static void GetRatingForRatedEntity(int categoryID, int ratedEntityID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_RATING_FOR_RATED_ENTITY,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("categoryID", categoryID);
		obj.AddParam("ratedEntityID", ratedEntityID);
		obj._URL = mRatingURL + "GetRatingForRatedEntity";
		ServiceCall<int>.Create(obj)?.DoGet();
	}

	public static void SubmitScore(int categoryID, int entityID, int scoreValue, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SUBMIT_SCORE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("categoryID", categoryID);
		serviceRequest.AddParam("entityID", entityID);
		serviceRequest.AddParam("score", scoreValue);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + categoryID + entityID + scoreValue));
		serviceRequest._URL = mScoreURL + "SetScore";
		ServiceCall<ScoreInfo>.Create(serviceRequest)?.DoSet();
	}

	public static void SendAccountActivationReminder(string username, string password, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.SEND_ACCOUNT_ACTIVATION_REMINDER;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		string text = TripleDES.EncryptUnicode(UtUtilities.SerializeToString(new ActivationReminderRequest
		{
			Email = username,
			Pwd = password
		}), mSecret);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("apiToken", mUserToken);
		serviceRequest.AddParam("activationReminderRequest", text);
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		serviceRequest._URL = mRegistrationV3URL + "SendAccountActivationReminder";
		ServiceCall<ActivationReminderResponse>.Create(serviceRequest)?.DoSet();
	}

	public static void ClearScore(int categoryID, int entityID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.CLEAR_SCORE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("categoryID", categoryID);
		serviceRequest.AddParam("entityID", entityID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + categoryID + entityID));
		serviceRequest._URL = mScoreURL + "DeleteEntityScoreData";
		ServiceCall<bool>.Create(serviceRequest, ServiceCallType.NORESULT)?.DoSet();
	}

	public static void GetTopScore(int categoryID, int entityID, int numRecords, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TOP_SCORE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("categoryID", categoryID);
		obj.AddParam("entityID", entityID);
		obj.AddParam("count", numRecords);
		obj._URL = mScoreURL + "GetEntityTopScores";
		ServiceCall<ScoreInfo[]>.Create(obj)?.DoGet();
	}

	public static void GetTracksByUserID(int categoryID, string uID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TRACKS_BY_USER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("category", categoryID);
		obj.AddParam("userId", uID);
		obj._URL = mTrackURL + "GetTracksByUserID";
		ServiceCall<TrackInfo[]>.Create(obj)?.DoGet();
	}

	public static void GetTracksByTrackIDs(int[] tIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.GET_TRACKS_BY_IDS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = "";
		if (tIDs.Length != 0)
		{
			text = tIDs[0].ToString();
			for (int i = 1; i < tIDs.Length; i++)
			{
				text = text + "," + tIDs[i];
			}
		}
		serviceRequest.AddParam("trackIDs", text);
		serviceRequest._URL = mTrackURL + "GetTracksByIDs";
		ServiceCall<TrackInfo[]>.Create(serviceRequest)?.DoGet();
	}

	public static void GetTrackElements(int tID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_TRACK_ELEMENTS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("trackID", tID);
		obj._URL = mTrackURL + "GetTrackElements";
		ServiceCall<TrackElement[]>.Create(obj)?.DoGet();
	}

	public static void SetTrack(TrackSetRequest tr, Texture2D inImage, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_TRACK,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(tr);
		serviceRequest.AddParam("contentXML", text);
		byte[] inArray = new byte[0];
		if (inImage != null)
		{
			inArray = inImage.EncodeToPNG();
		}
		string text2 = Convert.ToBase64String(inArray);
		serviceRequest.AddParam("imageFile", text2);
		string text3 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text3);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text3 + mSecret + mUserToken + text + text2));
		serviceRequest._URL = mTrackURL + "SetTrack";
		ServiceCall<TrackInfo>.Create(serviceRequest)?.DoSet();
	}

	public static void DeleteTrack(int tID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.DELETE_TRACK,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("trackID", tID);
		string text = Ticks.ToString();
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + tID));
		obj._URL = mTrackURL + "DeleteTrack";
		ServiceCall<bool>.Create(obj)?.DoSet();
	}

	public static void RemoveUserWithoutTrack(string[] uIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.REMOVE_USER_WO_TRACK,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = "";
		foreach (string text2 in uIDs)
		{
			if (text.Length > 0)
			{
				text += ",";
			}
			text += text2;
		}
		serviceRequest.AddParam("commaSeperatedUserIdList", text);
		serviceRequest._URL = mTrackURL + "RemoveUserWithoutTrack";
		ServiceCall<string[]>.Create(serviceRequest)?.DoSet();
	}

	public static void SetUserAchievementTask(AchievementTask[] inAchievementTasks, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_ACHIEVEMENT_TASK,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string plaintext = UtUtilities.ProcessSendObject(new AchievementTaskSetRequest
		{
			AchievementTaskSet = inAchievementTasks
		});
		plaintext = TripleDES.EncryptUnicode(plaintext, mSecret).Trim();
		obj.AddParam("achievementTaskSetRequest", plaintext);
		string text = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + plaintext);
		obj.AddParam("ticks", text);
		obj.AddParam("signature", md5Hash);
		obj._URL = mAchievementV2URL + "SetUserAchievementTask";
		ServiceCall<ArrayOfAchievementTaskSetResponse>.Create(obj)?.DoSet();
	}

	public static void SetAchievementTaskByUserID(string userID, int taskID, int achievementInfoID, string relatedID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_ACHIEVEMENT_TASK_BY_USERID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("userId", userID);
		serviceRequest.AddParam("taskId", taskID);
		serviceRequest.AddParam("achievementInfoID", achievementInfoID);
		serviceRequest.AddParam("relatedID", relatedID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + userID + taskID + achievementInfoID + relatedID));
		serviceRequest._URL = mAchievementURL + "SetAchievementTaskByUserID";
		ServiceCall<AchievementTaskSetResponse>.Create(serviceRequest)?.DoSet();
	}

	public static void GetUserAchievementTask(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACHIEVEMENT_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mAchievementURL + "GetUserAchievementTask";
		ServiceCall<ArrayOfUserAchievementTask>.Create(obj)?.DoGet();
	}

	public static void GetLocaleData(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_LOCALE_DATA,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mLocaleURL + "GetLocaleData";
		ServiceCall<LocaleData>.Create(obj)?.DoGet();
	}

	public static void GetLocaleData(WsServiceEventHandler inCallback, string inLocale, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_LOCALE_DATA,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		GetLocaleRequest inObject = new GetLocaleRequest
		{
			Locale = inLocale
		};
		AddCommon(obj);
		obj.AddParam("getLocaleRequest", UtUtilities.ProcessSendObject(inObject));
		obj._URL = mLocaleV2URL + "GetLocaleData";
		ServiceCall<LocaleData>.Create(obj)?.DoGet();
	}

	public static void GetGamePlayDataForDateRange(DateTime startDate, DateTime endDate, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAMEPLAY_DATA_FOR_DATE_RANGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("startDate", startDate.ToString(UtUtilities.GetCultureInfo("en-US")));
		obj.AddParam("endDate", endDate.ToString(UtUtilities.GetCultureInfo("en-US")));
		obj._URL = mContentURL + "GetGamePlayDataForDateRange";
		ServiceCall<ArrayOfGamePlayData>.Create(obj)?.DoGet();
	}

	public static void GetUserProfile(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_PROFILE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mProfileURL + "GetUserProfile";
		ServiceCall<UserProfileData>.Create(obj)?.DoGet();
	}

	public static void GetUserProfileByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_PROFILE_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mProfileURL + "GetUserProfileByUserID";
		ServiceCall<UserProfileData>.Create(obj)?.DoGet();
	}

	public static void GetQuestions(bool activeOnly, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_QUESTIONS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("activeOnly", activeOnly.ToString());
		obj._URL = mProfileURL + "GetQuestions";
		ServiceCall<ProfileQuestionData>.Create(obj)?.DoGet();
	}

	public static void GetUserAnswers(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_ANSWERS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mProfileURL + "GetUserAnswers";
		ServiceCall<UserAnswerData>.Create(obj)?.DoGet();
	}

	public static void SetUserProfileAnswers(int[] answers, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_PROFILE_ANSWERS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = answers[0].ToString();
		for (int i = 1; i < answers.Length; i++)
		{
			text = text + "," + answers[i];
		}
		serviceRequest.AddParam("profileAnswerIDs", text);
		serviceRequest._URL = mProfileURL + "SetUserProfileAnswers";
		ServiceCall<bool>.Create(serviceRequest, ServiceCallType.NORESULT)?.DoSet();
	}

	public static void SetUserGender(Gender gender, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_USER_GENDER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("gender", gender.ToString());
		obj._URL = mProfileURL + "SetUserGender";
		ServiceCall<bool>.Create(obj, ServiceCallType.NORESULT)?.DoSet();
	}

	public static void GetBuddyStatusList(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_BUDDY_STATUS_LIST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mMessageURL + "GetBuddyStatusList";
		ServiceCall<ArrayOfMessage>.Create(obj)?.DoGet();
	}

	public static void GetConversationByMessageID(int messageID, ConversationFilter filter, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_CONVERSATION_BY_MESSAGE_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("messageID", messageID);
		obj.AddParam("filter", filter.ToString());
		obj._URL = mMessageURL + "GetConversationByMessageID";
		ServiceCall<Conversation>.Create(obj)?.DoGet();
	}

	public static void GetMessageBoard(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_MESSAGE_BOARD,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mMessageURL + "GetMessageBoard";
		ServiceCall<MessageList>.Create(obj)?.DoGet();
	}

	public static void GetStatusConversation(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_STATUS_CONVERSATION,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mMessageURL + "GetStatusConversation";
		ServiceCall<Conversation>.Create(obj)?.DoGet();
	}

	public static void RemoveMessageFromBoard(int messageID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REMOVE_MESSAGE_FROM_BOARD,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("messageID", messageID);
		obj._URL = mMessageURL + "RemoveMessageFromBoard";
		ServiceCall<bool>.Create(obj, ServiceCallType.NORESULT)?.DoSet();
	}

	public static void InitiateChallenge(Guid[] userIDs, int gameID, int gameLevelID, int gameDifficultyID, int messageID, int points, int expiryDurationDays, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.INITIATE_CHALLENGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(userIDs);
		serviceRequest.AddParam("userIDs", text);
		serviceRequest.AddParam("gameID", gameID);
		serviceRequest.AddParam("gameLevelID", gameLevelID);
		serviceRequest.AddParam("gameDifficultyID", gameDifficultyID);
		serviceRequest.AddParam("messageID", messageID);
		serviceRequest.AddParam("points", points);
		serviceRequest.AddParam("expiryDurationDays", expiryDurationDays);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text + gameID + gameLevelID + gameDifficultyID + messageID + points + expiryDurationDays));
		serviceRequest._URL = mChallengeURL + "InitiateChallenge";
		ServiceCall<ChallengeInfo>.Create(serviceRequest)?.DoGet();
	}

	public static void AcceptChallenge(int challengeID, int messageID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.ACCEPT_CHALLENGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("challengeID", challengeID);
		serviceRequest.AddParam("messageID", messageID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + challengeID + messageID));
		serviceRequest._URL = mChallengeURL + "AcceptChallenge";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void RejectChallenge(int challengeID, int messageID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.REJECT_CHALLENGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("challengeID", challengeID);
		serviceRequest.AddParam("messageID", messageID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + challengeID + messageID));
		serviceRequest._URL = mChallengeURL + "RejectChallenge";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void RespondToChallenge(int challengeID, int messageID, int points, int gamePointsType, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.RESPOND_TO_CHALLENGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("challengeID", challengeID);
		serviceRequest.AddParam("messageID", messageID);
		serviceRequest.AddParam("points", points);
		serviceRequest.AddParam("pointsType", gamePointsType);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + challengeID + messageID + points + gamePointsType));
		serviceRequest._URL = mChallengeURL + "RespondToChallenge";
		ServiceCall<ChallengeInfo>.Create(serviceRequest)?.DoSet();
	}

	public static void GetActiveChallenges(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ACTIVE_CHALLENGES,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mChallengeURL + "GetActiveChallenges";
		ServiceCall<ChallengeInfo[]>.Create(obj)?.DoGet();
	}

	public static void AcceptTrade(int containerID, int myInventoryItemID, string otherUserID, int otherInventoryItemID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.ACCEPT_TRADE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("ContainerId", containerID);
		serviceRequest.AddParam("myInventoryItemID", myInventoryItemID);
		serviceRequest.AddParam("otherUserID", otherUserID);
		serviceRequest.AddParam("otherInventoryItemID", otherInventoryItemID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + myInventoryItemID + otherUserID + otherInventoryItemID));
		serviceRequest._URL = mContentURL + "AcceptTrade";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void ChatAuthorizationRequest(string parentEmail, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.CHAT_AUTHORIZATION_REQUEST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("parentEmailID", parentEmail);
		obj._URL = mChatURL + "ChatAuthorizationRequest";
		ServiceCall<RequestChatAuthorizationResponse>.Create(obj)?.DoGet();
	}

	public static void ReportUser(string userID, int reason, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REPORT_USER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("reportUserID", userID);
		obj.AddParam("reportReason", reason);
		obj._URL = mChatURL + "ReportUser";
		ServiceCall<bool>.Create(obj, ServiceCallType.NORESULT)?.DoGet();
	}

	public static void GetGameProgress(int gameid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAMEPROGRESS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("gameId", gameid);
		obj._URL = mContentURL + "GetGameProgress";
		ServiceCall<GameProgress>.Create(obj)?.DoGet();
	}

	public static void GetGameProgressByUserID(string userid, int gameid, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GAMEPROGRESS_BY_USERID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userid);
		obj.AddParam("gameId", gameid);
		obj._URL = mContentURL + "GetGameProgressByUserID";
		ServiceCall<GameProgress>.Create(obj)?.DoGet();
	}

	public static void SetGameProgress(int gameid, GameProgress inData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SET_GAMEPROGRESS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("gameId", gameid);
		string text = UtUtilities.ProcessSendObject(inData);
		serviceRequest.AddParam("xmlDocumentData", text);
		string text2 = Ticks.ToString();
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + gameid + text));
		serviceRequest._URL = mContentURL + "SetGameProgress";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void LoginParent(string userID, string userKey, ExternalAuthProvider provider, string childUserID, string locale, WsServiceEventHandler inCallback, object inUserData, UserPolicy userPolicy = null)
	{
		LoginParent("", "", userID, userKey, provider, childUserID, locale, inCallback, inUserData, userPolicy);
	}

	public static void LoginParent(string userName, string password, string userID, string userKey, ExternalAuthProvider provider, string childUserID, string locale, WsServiceEventHandler inCallback, object inUserData, UserPolicy userPolicy = null)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.LOGIN_PARENT;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		ParentLoginData parentLoginData = new ParentLoginData
		{
			UserName = userName,
			Password = password,
			FaceBookUserId = null,
			FacebookAccessToken = "",
			UserPolicy = userPolicy,
			Locale = locale
		};
		if (childUserID != "")
		{
			parentLoginData.ChildUserID = new Guid(childUserID);
		}
		parentLoginData.ExternalAuthProvider = provider;
		parentLoginData.ExternalUserID = userID;
		parentLoginData.ExternalAuthData = UtUtilities.SerializeToString(new ExternalAuthData
		{
			AccessToken = userKey
		});
		string plaintext = UtUtilities.SerializeToString(parentLoginData);
		serviceRequest.AddParam("parentLoginData", TripleDES.EncryptUnicode(plaintext, mSecret));
		serviceRequest._URL = mAuthenticationV3URL + "LoginParent";
		ServiceCall<ParentLoginInfo>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoGet();
	}

	public static void LoginParent(string username, string password, string facebookID, string facebookAccessToken, string childUserID, string locale, WsServiceEventHandler inCallback, object inUserData, UserPolicy userPolicy = null)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.LOGIN_PARENT;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		ParentLoginData parentLoginData = new ParentLoginData
		{
			UserName = username,
			Password = password
		};
		if (string.IsNullOrEmpty(facebookID))
		{
			parentLoginData.FaceBookUserId = null;
		}
		else
		{
			parentLoginData.FaceBookUserId = long.Parse(facebookID);
		}
		parentLoginData.FacebookAccessToken = facebookAccessToken;
		parentLoginData.UserPolicy = userPolicy;
		parentLoginData.Locale = locale;
		if (childUserID != "")
		{
			parentLoginData.ChildUserID = new Guid(childUserID);
		}
		string plaintext = UtUtilities.SerializeToString(parentLoginData);
		serviceRequest.AddParam("parentLoginData", TripleDES.EncryptUnicode(plaintext, mSecret));
		serviceRequest._URL = mAuthenticationV3URL + "LoginParent";
		ServiceCall<ParentLoginInfo>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoGet();
	}

	public static void LoginGuest(string appName, string userName, int? age, string locale, WsServiceEventHandler inCallback, object inUserData, UserPolicy userPolicy = null)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.LOGIN_GUEST;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		GuestLoginData inObject = new GuestLoginData
		{
			Locale = locale,
			UserName = userName,
			SubscriptionID = ProductSettings.pInstance._SubscriptionID,
			UserPolicy = userPolicy,
			Age = age
		};
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("appName", appName);
		string inVal = TripleDES.EncryptUnicode(UtUtilities.ProcessSendObject(inObject), mSecret);
		serviceRequest.AddParam("guestLoginData", inVal);
		serviceRequest._URL = mAuthenticationV3URL + "LoginGuest";
		ServiceCall<ParentLoginInfo>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoGet();
	}

	public static void LoginChild(string parentToken, string childUserID, string locale, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.LOGIN_CHILD,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("parentApiToken", parentToken);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		string text2 = TripleDES.EncryptUnicode(childUserID, mSecret);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + parentToken + text2 + locale));
		serviceRequest.AddParam("childUserID", text2);
		serviceRequest.AddParam("locale", locale);
		serviceRequest._URL = mAuthenticationURL + "LoginChild";
		ServiceCall<string>.Create(serviceRequest, ServiceCallType.CHILDDATA)?.DoGet();
	}

	public static void AuthenticateUser(string username, string password, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.AUTHENTICATE_USER;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("username", TripleDES.EncryptUnicode(username, mSecret));
		serviceRequest.AddParam("password", TripleDES.EncryptUnicode(password, mSecret));
		serviceRequest._URL = mAuthenticationV3URL + "AuthenticateUser";
		ServiceCall<bool>.Create(serviceRequest)?.DoGet();
	}

	public static void RecoverPassword(string email, string username, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.RECOVER_PASSWORD;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("email", email);
		serviceRequest.AddParam("username", username);
		serviceRequest._URL = mAuthenticationV3URL + "RecoverPassword";
		ServiceCall<MembershipUserStatus>.Create(serviceRequest)?.DoGet();
	}

	public static void ResetPassword(string email, string username, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.RESET_PASSWORD;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("email", email);
		serviceRequest.AddParam("username", username);
		serviceRequest._URL = mAuthenticationV3URL + "ResetPassword";
		ServiceCall<MembershipUserStatus>.Create(serviceRequest)?.DoGet();
	}

	public static void RecoverAccount(string email, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.RECOVER_ACCOUNT;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("email", email);
		serviceRequest._URL = mAuthenticationURL + "RecoverAccount";
		ServiceCall<MembershipUserStatus>.Create(serviceRequest)?.DoGet();
	}

	public static void RegisterGuest(string appName, string guestUserName, int age, string email, string password, string childName, string culture, UserPolicy userpolicy, EmailNotification emailNotification, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.REGISTER_PARENT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("appName", appName);
		ParentRegistrationData parentRegistrationData = new ParentRegistrationData();
		parentRegistrationData.Email = email;
		parentRegistrationData.Locale = culture;
		parentRegistrationData.Password = password;
		parentRegistrationData.ReceivesEmail = false;
		parentRegistrationData.SubscriptionID = ProductSettings.pInstance._SubscriptionID;
		parentRegistrationData.ChildList = new ChildRegistrationData[2]
		{
			new ChildRegistrationData
			{
				Age = age,
				ChildName = childName,
				BirthDate = DateTime.MinValue,
				Gender = "",
				Password = password
			},
			new ChildRegistrationData
			{
				GuestUserName = guestUserName,
				IsGuest = true
			}
		};
		parentRegistrationData.UserPolicy = userpolicy;
		parentRegistrationData.EmailNotification = emailNotification;
		string inVal = TripleDES.EncryptUnicode(UtUtilities.SerializeToString(parentRegistrationData), mSecret);
		serviceRequest.AddParam("parentRegistrationData", inVal);
		serviceRequest._URL = mRegistrationV3URL + "RegisterParent";
		ServiceCall<RegistrationResult>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoSet();
	}

	public static void RegisterParent(string appName, ParentRegistrationData parentData, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.REGISTER_PARENT;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("appName", appName);
		string text = UtUtilities.SerializeToString(parentData);
		Debug.LogWarning("parentData = " + text);
		string inVal = TripleDES.EncryptUnicode(text, mSecret);
		serviceRequest.AddParam("parentRegistrationData", inVal);
		serviceRequest._URL = mRegistrationV3URL + "RegisterParent";
		ServiceCall<RegistrationResult>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoSet();
	}

	public static void DeleteProfile(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.DELETE_PROFILE;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("apiToken", ProductConfig.pToken);
		serviceRequest.AddParam("userID", userID);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		string md5Hash = WsMD5Hash.GetMd5Hash(text + mSecret + ProductConfig.pToken + userID);
		serviceRequest.AddParam("signature", md5Hash);
		serviceRequest._URL = mRegistrationV3URL + "DeleteProfile";
		ServiceCall<DeleteProfileStatus>.Create(serviceRequest)?.DoGet();
	}

	public static void DeleteAccount(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.DELETE_ACCOUNT;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("apiToken", ProductConfig.pToken);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		string md5Hash = WsMD5Hash.GetMd5Hash(text + mSecret + ProductConfig.pToken);
		serviceRequest.AddParam("signature", md5Hash);
		serviceRequest._URL = mAuthenticationURL + "DeleteAccountNotification";
		ServiceCall<MembershipUserStatus>.Create(serviceRequest)?.DoGet();
	}

	public static void RegisterChild(string appName, string parentApiToken, string childName, DateTime birthDate, string gender, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.REGISTER_CHILD;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("appName", appName);
		string text = Ticks.ToString();
		serviceRequest.AddParam("parentApiToken", parentApiToken);
		serviceRequest.AddParam("ticks", text);
		string text2 = TripleDES.EncryptUnicode(UtUtilities.SerializeToString(new ChildRegistrationData
		{
			ChildName = childName,
			BirthDate = birthDate,
			Gender = gender
		}), mSecret);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + parentApiToken + text2));
		serviceRequest.AddParam("childRegistrationData", text2);
		serviceRequest._URL = mRegistrationV3URL + "RegisterChild";
		ServiceCall<MembershipUserStatus>.Create(serviceRequest)?.DoGet();
	}

	public static void RegisterChild(string appName, string parentApiToken, string childName, DateTime birthDate, string gender, bool isSuggestAvatarName, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.REGISTER_CHILD;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("appName", appName);
		string text = Ticks.ToString();
		serviceRequest.AddParam("parentApiToken", parentApiToken);
		serviceRequest.AddParam("ticks", text);
		string text2 = TripleDES.EncryptUnicode(UtUtilities.SerializeToString(new ChildRegistrationData
		{
			ChildName = childName,
			BirthDate = birthDate,
			Gender = gender,
			IsSuggestAvatarName = isSuggestAvatarName
		}), mSecret);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + parentApiToken + text2));
		serviceRequest.AddParam("childRegistrationData", text2);
		serviceRequest._URL = mRegistrationV4URL + "RegisterChild";
		ServiceCall<RegistrationResult>.Create(serviceRequest, ServiceCallType.DECRYPT)?.DoGet();
	}

	public static void LogOffParent(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.LOG_OFF_PARENT;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("apiToken", mUserToken);
		serviceRequest._URL = mContentURL + "SetUserLogoff";
		ServiceCall<bool>.Create(serviceRequest, ServiceCallType.NORESULT)?.DoSet();
	}

	public static void RegisterAppInstall(string inUserID, string inInstallID, string inDeviceTokenID, string inDeviceTypeID, string inPlatformID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.REGISTER_APP_INSTALL;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", ProductConfig.pApiKey);
		serviceRequest.AddParam("userID", inUserID);
		serviceRequest.AddParam("appInstallID", inInstallID);
		serviceRequest.AddParam("deviceTokenID", inDeviceTokenID);
		serviceRequest.AddParam("deviceTypeID", inDeviceTypeID);
		serviceRequest.AddParam("platformID", inPlatformID);
		serviceRequest._URL = mPushNotificationURL + "RegisterAppInstall";
		ServiceCall<RegistrationValidateResult>.Create(serviceRequest)?.DoGet();
	}

	public static void GetUsersWithPet(string inPetTypeFilter, bool inActiveOnly, int inUserCount, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_USER_WITH_PETTYPE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("petTypeFilter", inPetTypeFilter);
		obj.AddParam("activeOnly", inActiveOnly.ToString());
		obj.AddParam("count", inUserCount);
		obj._URL = mContentURL + "GetUsersWithPets";
		ServiceCall<ArrayOfRaisedPetUser>.Create(obj)?.DoGet();
	}

	public static void CheckProductVersion(string inVersion, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.CHECK_PRODUCT_VERSION,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("version", inVersion);
		obj._URL = mItemStoreURL + "CheckProductVersion";
		ServiceCall<VersionStatus>.Create(obj)?.DoGet();
	}

	public static void CheckProductVersionV2(string inVersion, string inLocale, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.CHECK_PRODUCT_VERSION;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest.AddParam("version", inVersion);
		serviceRequest.AddParam("locale", inLocale);
		serviceRequest._URL = mItemStoreURL + "CheckProductVersion";
		ServiceCall<VersionStatus>.Create(serviceRequest)?.DoGet();
	}

	public static void GetProfileTagAll(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest();
		serviceRequest._Type = WsServiceType.GET_PROFILE_TAG_ALL;
		serviceRequest._EventDelegate = inCallback;
		serviceRequest._UserData = inUserData;
		serviceRequest.AddParam("apiKey", mApiKey);
		serviceRequest._URL = mProfileURL + "GetProfileTagAll";
		ServiceCall<ProfileTag[]>.Create(serviceRequest)?.DoGet();
	}

	public static void SendFriendInvite(string inviter, string[] emailIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.SEND_FRIEND_INVITE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		string text = UtUtilities.ProcessSendObject(emailIDs);
		serviceRequest.AddParam("firstNameInviter", inviter);
		serviceRequest.AddParam("friendEmailds", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + inviter + text);
		serviceRequest.AddParam("ticks", text2);
		serviceRequest.AddParam("signature", md5Hash);
		serviceRequest._URL = mInviteV2URL + "SendFriendInvite";
		ServiceCall<InviteData>.Create(serviceRequest)?.DoGet();
	}

	public static void SendFriendInviteRequest(FriendInviteRequest friendInviteRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SEND_FRIEND_INVITE_REQUEST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(friendInviteRequest);
		obj.AddParam("friendInviteRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mInviteV2URL + "SendFriendInviteRequest";
		ServiceCall<InviteData>.Create(obj)?.DoGet();
	}

	public static void SendFriendInviteRegister(InviteProcessRequest inviteProcessRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SEND_FRIEND_INVITE_REGISTER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inviteProcessRequest);
		obj.AddParam("inviteProcessRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mInviteV2URL + "PostFriendInviteRegisterProcessing";
		ServiceCall<InviteData>.Create(obj)?.DoGet();
	}

	public static void PurchaseGift(string receiverUserID, int itemID, int storeID, int inContainerID, int currencyType, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest serviceRequest = new ServiceRequest
		{
			_Type = WsServiceType.PURCHASE_GIFT,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(serviceRequest);
		serviceRequest.AddParam("receiverUserID", receiverUserID);
		serviceRequest.AddParam("itemId", itemID);
		serviceRequest.AddParam("storeId", storeID);
		serviceRequest.AddParam("ContainerId", inContainerID);
		serviceRequest.AddParam("currencyType", currencyType);
		string text = Ticks.ToString();
		serviceRequest.AddParam("ticks", text);
		serviceRequest.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + receiverUserID + itemID + storeID + inContainerID + currencyType));
		serviceRequest._URL = mContentURL + "PurchaseGift";
		ServiceCall<bool>.Create(serviceRequest)?.DoSet();
	}

	public static void GetCombinedListMessage(string userID, string filterName, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_COMBINED_LIST_MESSAGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj.AddParam("filterName", filterName);
		obj._URL = mMessageURL + "GetCombinedListMessage";
		ServiceCall<CombinedListMessage[]>.Create(obj)?.DoGet();
	}

	public static void SetNextItemState(SetNextItemStateRequest inItemStateRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_NEXT_ITEMSTATE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inItemStateRequest);
		obj.AddParam("setNextItemStateRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mContentURL + "SetNextItemState";
		ServiceCall<SetNextItemStateResult>.Create(obj)?.DoSet();
	}

	public static void SetSpeedUpItem(SetSpeedUpItemRequest speedupItemRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SET_SPEEDUP_ITEM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(speedupItemRequest);
		obj.AddParam("setSpeedUpItemRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mContentURL + "SetSpeedUpItem";
		ServiceCall<SetSpeedUpItemResult>.Create(obj)?.DoSet();
	}

	public static void CreateGroup(CreateGroupRequest createGroupRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.CREATE_GROUP,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(createGroupRequest);
		obj.AddParam("groupCreateRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "CreateGroup";
		ServiceCall<CreateGroupResult>.Create(obj)?.DoSet();
	}

	public static void EditGroup(EditGroupRequest editGroupRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.EDIT_GROUP,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(editGroupRequest);
		obj.AddParam("groupEditRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "EditGroup";
		ServiceCall<EditGroupResult>.Create(obj)?.DoSet();
	}

	public static void JoinGroup(JoinGroupRequest joinGroupRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.JOIN_GROUP,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(joinGroupRequest);
		obj.AddParam("groupJoinRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "JoinGroup";
		ServiceCall<GroupJoinResult>.Create(obj)?.DoSet();
	}

	public static void LeaveGroup(LeaveGroupRequest leaveGroupRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.LEAVE_GROUP,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(leaveGroupRequest);
		obj.AddParam("groupLeaveRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "LeaveGroup";
		ServiceCall<LeaveGroupResult>.Create(obj)?.DoSet();
	}

	public static void GetGroups(GetGroupsRequest getGroupsRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GROUPS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(getGroupsRequest);
		obj.AddParam("getGroupsRequest", inVal);
		obj._URL = mGroupV2URL + "GetGroups";
		ServiceCall<GetGroupsResult>.Create(obj)?.DoGet();
	}

	public static void RemoveMember(RemoveMemberRequest removeMemberRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.REMOVE_MEMBER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(removeMemberRequest);
		obj.AddParam("removeMemberRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "RemoveMember";
		ServiceCall<RemoveMemberResult>.Create(obj)?.DoSet();
	}

	public static void AuthorizeJoinRequest(AuthorizeJoinRequest authorizeJoinRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.AUTHORIZE_JOIN_REQUEST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(authorizeJoinRequest);
		obj.AddParam("authorizeJoinRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "AuthorizeJoinRequest";
		ServiceCall<AuthorizeJoinResult>.Create(obj)?.DoSet();
	}

	public static void AssignRole(AssignRoleRequest assignRoleRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.ASSIGN_ROLE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(assignRoleRequest);
		obj.AddParam("assignRoleRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "AssignRole";
		ServiceCall<AssignRoleResult>.Create(obj)?.DoSet();
	}

	public static void GetPendingJoinRequest(GetPendingJoinRequest getPendingJoinRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_PENDING_JOIN_REQUEST,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(getPendingJoinRequest);
		obj.AddParam("getPendingJoinRequest", inVal);
		obj._URL = mGroupV2URL + "GetPendingJoinRequest";
		ServiceCall<GetPendingJoinResult>.Create(obj)?.DoGet();
	}

	public static void InvitePlayer(InvitePlayerRequest invitePlayerRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.INVITE_PLAYER,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(invitePlayerRequest);
		obj.AddParam("invitePlayerRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "InvitePlayer";
		ServiceCall<InvitePlayerResult>.Create(obj)?.DoSet();
	}

	public static void UpdateInvite(UpdateInviteRequest updateInviteRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.UPDATE_INVITE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(updateInviteRequest);
		obj.AddParam("updateInviteRequest", text);
		string text2 = Ticks.ToString();
		string md5Hash = WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", md5Hash);
		obj._URL = mGroupV2URL + "UpdateInvite";
		ServiceCall<UpdateInviteResult>.Create(obj)?.DoSet();
	}

	public static void GetGroupsByUserID(string userID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GROUPS_BY_USER_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("userId", userID);
		obj._URL = mGroupURL + "GetGroupsByUserID";
		ServiceCall<Group[]>.Create(obj)?.DoGet();
	}

	public static void GetGroupsByGroupType(GroupType inType, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_GROUPS_BY_GROUP_TYPE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("groupType", inType.ToString());
		obj._URL = mGroupURL + "GetGroupsByGroupType";
		ServiceCall<Group[]>.Create(obj)?.DoGet();
	}

	public static void GetMembersByGroupID(string inGroupID, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_MEMBERS_BY_GROUP_ID,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("groupID", inGroupID);
		obj._URL = mGroupURL + "GetMembersByGroupID";
		ServiceCall<GroupMember[]>.Create(obj)?.DoGet();
	}

	public static void ValidateName(NameValidationRequest nameValidationRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.VALIDATE_NAME,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.ProcessSendObject(nameValidationRequest);
		obj.AddParam("nameValidationRequest", inVal);
		obj._URL = mContentV2URL + "ValidateName";
		ServiceCall<NameValidationResponse>.Create(obj)?.DoSet();
	}

	public static void ValidatePrizeCode(string Code, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.VALIDATE_PRIZE_CODE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("code", Code);
		obj._URL = mPrizeCodeURL + "ValidatePrizeCode";
		ServiceCall<SubmissionResult>.Create(obj)?.DoSet();
	}

	public static void SubmitPrizeCode(string Code, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SUBMIT_PRIZE_CODE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj.AddParam("code", Code);
		obj._URL = mPrizeCodeURL + "SubmitPrizeCode";
		ServiceCall<SubmissionResult>.Create(obj)?.DoSet();
	}

	public static void SubmitPrizeCode(PrizeCodeSubmitRequest inPrizeCodeRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.SUBMIT_PRIZE_CODE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(inPrizeCodeRequest);
		obj.AddParam("contentXML", text);
		string text2 = Ticks.ToString();
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mPrizeCodeV2URL + "SubmitPrizeCode";
		ServiceCall<PrizeCodeSubmitResponse>.Create(obj)?.DoSet();
	}

	public static void ApplyRewards(ApplyRewardsRequest request, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.APPLY_REWARD,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(request);
		string text2 = Ticks.ToString();
		obj.AddParam("request", text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "ApplyRewards";
		ServiceCall<ApplyRewardsResponse>.Create(obj)?.DoSet();
	}

	public static void ProcessRewardedItems(ProcessRewardedItemsRequest request, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.PROCESS_REWARDS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = UtUtilities.ProcessSendObject(request);
		string text2 = Ticks.ToString();
		obj.AddParam("request", text);
		obj.AddParam("ticks", text2);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text2 + mSecret + mUserToken + text));
		obj._URL = mContentV2URL + "ProcessRewardedItems";
		ServiceCall<ProcessRewardedItemsResponse>.Create(obj)?.DoSet();
	}

	public static void FuseItems(FuseItemsRequest request, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.FUSE_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = Ticks.ToString();
		string text2 = UtUtilities.ProcessSendObject(request);
		obj.AddParam("fuseItemsRequest", text2);
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + text2));
		obj._URL = mContentV2URL + "FuseItems";
		ServiceCall<FuseItemsResponse>.Create(obj)?.DoSet();
	}

	public static void BattleReadyItems(AddBattleItemsRequest request, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.BATTLE_READY_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = Ticks.ToString();
		string text2 = UtUtilities.ProcessSendObject(request);
		obj.AddParam("request", text2);
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + text2));
		obj._URL = mContentV2URL + "AddBattleItems";
		ServiceCall<AddBattleItemsResponse>.Create(obj)?.DoSet();
	}

	public static void ItemExchange(ProcessExchangeItemRequest itemRequest, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.ITEM_EXCHANGE,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string text = Ticks.ToString();
		string text2 = UtUtilities.ProcessSendObject(itemRequest);
		obj.AddParam("request", text2);
		obj.AddParam("ticks", text);
		obj.AddParam("signature", WsMD5Hash.GetMd5Hash(text + mSecret + mUserToken + text2));
		obj._URL = mContentURL + "ProcessExchangeItem";
		ServiceCall<ExchangeItemResponse>.Create(obj)?.DoSet();
	}

	public static void GetAllExchangeItems(WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_ALL_EXCHANGE_ITEMS,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		obj._URL = mItemStoreURL + "GetAllExchangeItemList";
		ServiceCall<ArrayOfItemExchange>.Create(obj)?.DoGet();
	}

	public static void GetExchangeItemListByItem(int[] ItemIDs, WsServiceEventHandler inCallback, object inUserData)
	{
		ServiceRequest obj = new ServiceRequest
		{
			_Type = WsServiceType.GET_EXCHANGE_ITEM_LIST_BY_ITEM,
			_EventDelegate = inCallback,
			_UserData = inUserData
		};
		AddCommon(obj);
		string inVal = UtUtilities.SerializeToString(ItemIDs);
		obj.AddParam("itemids", inVal);
		obj._URL = mItemStoreURL + "GetExchangeItemListByItem";
		ServiceCall<ArrayOfItemExchange>.Create(obj)?.DoGet();
	}
}
