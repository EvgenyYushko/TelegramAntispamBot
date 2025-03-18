using System.Threading;
using BuisinessLogic.Handlers;
using Infrastructure.Common;
using Infrastructure.InjectSettings;
using ML_SpamClassifier.Interfaces;
using Moq;
using NUnit.Framework;
using ServiceLayer.Services.Authorization;
using ServiceLayer.Services.Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace TelegramAntispamBot.Tests
{
	public class HandleMessageServiceTests
	{
		private Mock<IDeleteMessageService> _deleteMessageService;
		private Mock<TelegramInject> _botClient;
		private Mock<IProfanityCheckerService> _profanityCheckerService;
		private Mock<ITelegramUserService> _userInfoService;
		private Mock<IUserService> _userSerivce;
		private Mock<ISpamDetector> _spamDetector;

		private CancellationToken _cancellationToken;
		private IHandleMessageService _service;

		[SetUp]
		public void Setup()
		{
			_deleteMessageService = new();
			_botClient = new();
			_profanityCheckerService = new();
			_cancellationToken = new();
			_userInfoService = new();
			_userSerivce = new();
			_spamDetector = new();
			_service = new HandleMessageService(_deleteMessageService.Object, _profanityCheckerService.Object, _userInfoService.Object
				, _userSerivce.Object
				, _spamDetector.Object);
		}

		[Test]
		public void IfEmptyMessage_Skip()
		{
			//Arrange
			var updateType = It.IsAny<UpdateType>();
			var update = new Update { Message = null };

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, updateType, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(), It.IsAny<CancellationToken>(), "test", null),
				Times.Never);
		}

		[Test]
		public void IfEditedMessageFromOwnChannel_Skip()
		{
			//Arrange
			var update = new Update
			{
				EditedMessage = new Message
				{
					From = new User
					{
						FirstName = "Telegram"
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.EditedMessage, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), "test", null),
				Times.Never);
		}

		[Test]
		public void IfMessageFromChannel_Skip()
		{
			//Arrange
			var update = new Update
			{
				Message = new Message
				{
					From = new User
					{
						FirstName = "Telegram"
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.Message, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), "test", null),
				Times.Never);
		}

		[Test]
		public void IfEditedMessageWithLinkFromUserInWhiteList_Skip()
		{
			//Arrange
			var update = new Update
			{
				EditedMessage = new Message
				{
					From = new User
					{
						FirstName = "testUserName",
						Username = BotSettings.WhiteList[0],
						IsBot = false
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}",
					Entities = new[]
					{
						new MessageEntity
						{
							Type = MessageEntityType.Url
						}
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.EditedMessage, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), "test", null),
				Times.Never);
		}

		[Test]
		public void IfMessageWithLinkFromUserInWhiteList_Skip()
		{
			//Arrange
			var update = new Update
			{
				Message = new Message
				{
					From = new User
					{
						FirstName = "testUserName",
						Username = BotSettings.WhiteList[0]
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}",
					Entities = new[]
					{
						new MessageEntity
						{
							Type = MessageEntityType.Url
						}
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.Message, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), "test", null),
				Times.Never);
		}

		[Test]
		public void IfEditedMessageWithNoCommentWordFromOwnChannel_Delete()
		{
			//Arrange
			var update = new Update
			{
				EditedMessage = new Message
				{
					From = new User
					{
						FirstName = "Telegram"
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}"
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.EditedMessage, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}

		[Test]
		public void IsMessageWithNoCommentWordFromOwnChannel_Delete()
		{
			//Arrange
			var update = new Update
			{
				Message = new Message
				{
					From = new User
					{
						FirstName = "Telegram"
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}"
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.Message, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), update.Message,
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}

		[Test]
		public void IfEditedMessageWithLinkFromUserNotInWhiteList_Delete()
		{
			//Arrange
			var update = new Update
			{
				EditedMessage = new Message
				{
					From = new User
					{
						FirstName = "testUserName",
						Username = "notInWhiteListUserName",
						IsBot = false
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}",
					Entities = new[]
					{
						new MessageEntity
						{
							Type = MessageEntityType.Url
						}
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.EditedMessage, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}

		[Test]
		public void IfMessageWithLinkFromUserNotInWhiteList_Delete()
		{
			//Arrange
			var update = new Update
			{
				Message = new Message
				{
					From = new User
					{
						FirstName = "testUserName",
						Username = "notInWhiteListUserName",
						IsBot = false
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}",
					Entities = new[]
					{
						new MessageEntity
						{
							Type = MessageEntityType.Url
						}
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.Message, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}

		[Test]
		public void IfEditedMessageWithLinkInCaptionFromUserNotInWhiteList_Delete()
		{
			//Arrange
			var update = new Update
			{
				EditedMessage = new Message
				{
					From = new User
					{
						FirstName = "testUserName",
						Username = "notInWhiteListUserName",
						IsBot = false
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}",
					CaptionEntities = new[]
					{
						new MessageEntity
						{
							Type = MessageEntityType.Url
						}
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.EditedMessage, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}

		[Test]
		public void IfMessageWithLinkInCaptionFromUserNotInWhiteList_Delete()
		{
			//Arrange
			var update = new Update
			{
				Message = new Message
				{
					From = new User
					{
						FirstName = "testUserName",
						Username = "notInWhiteListUserName",
						IsBot = false
					},
					Text = $"Test message with word {BotSettings.NoCommentWord}",
					CaptionEntities = new[]
					{
						new MessageEntity
						{
							Type = MessageEntityType.Url
						}
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.Message, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}

		[Test]
		public void IfEditedMessageFromChannelNotInWhiteList_Delete()
		{
			//Arrange
			var update = new Update
			{
				EditedMessage = new Message
				{
					Text = "example text",
					From = new User
					{
						IsBot = true
					},
					SenderChat = new Chat()
					{
						Username = "channelNotFromChannelsWhiteList"
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.EditedMessage, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}

		[Test]
		public void IfMessageFromChannelNotInWhiteList_Delete()
		{
			//Arrange
			var update = new Update
			{
				Message = new Message
				{
					Text = "example text",
					From = new User
					{
						IsBot = true
					},
					SenderChat = new Chat()
					{
						Username = "channelNotFromChannelsWhiteList"
					}
				}
			};

			//Act
			_service.HandleUpdateAsync(_botClient.Object, update, UpdateType.Message, _cancellationToken);

			//Assert
			_deleteMessageService.Verify(m =>
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(),
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage, null),
				Times.Once);
		}
	}
}