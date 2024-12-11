using System.Threading;
using Moq;
using NUnit.Framework;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramAntispamBot.BuisinessLogic.Services;
using TelegramAntispamBot.Common;
using TelegramAntispamBot.DataAccessLayer;
using TelegramAntispamBot.ServiceLayer.Services;

namespace TelegramAntispamBot.Tests
{
	public class HandleMessageServiceTests
	{
		private Mock<IDeleteMessageService> _deleteMessageService;
		private Mock<ITelegramBotClient> _botClient;
		private Mock<IProfanityCheckerService> _profanityCheckerService;
		private Mock<AppDbContext> AppDbContext;

		private CancellationToken _cancellationToken;
		private IHandleMessageService _service;

		[SetUp]
		public void Setup()
		{
			_deleteMessageService = new();
			_botClient = new();
			_profanityCheckerService = new();
			_cancellationToken = new();
			AppDbContext = new();
			_service = new HandleMessageService(_deleteMessageService.Object, _profanityCheckerService.Object, AppDbContext.Object);
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
					m.DeleteMessageAsync(It.IsAny<ITelegramBotClient>(), It.IsAny<Message>(), It.IsAny<CancellationToken>(), "test"),
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
						It.IsAny<CancellationToken>(), "test"),
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
						It.IsAny<CancellationToken>(), "test"),
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
						It.IsAny<CancellationToken>(), "test"),
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
						It.IsAny<CancellationToken>(), "test"),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
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
						It.IsAny<CancellationToken>(), BotSettings.InfoMessage),
				Times.Once);
		}
	}
}