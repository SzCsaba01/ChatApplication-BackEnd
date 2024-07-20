using Microsoft.EntityFrameworkCore;
using Project.Data.Data;
using Project.Data.Dto.Message;
using Project.Services.Business;
using Xunit;

namespace UnitTest;


public class Test {

    //private readonly DataContext _dataContext;
    //private readonly MessageService _messageService;

    //public Test() {
    //    var options = new DbContextOptionsBuilder<DataContext>().UseSqlServer("server=CSABA\\SQLEXPRESS;database=ChatApplication;trusted_connection=true;TrustServerCertificate=True").Options;
    //    _dataContext = new DataContext(options);
    //    _messageService = new MessageService(_dataContext);
    //}

    //[Fact]
    //public void GetMessages() {
    //    var messages = _messageService.GetMessageByIdAsync(Guid.Parse("DA7C27C5-9407-49BE-C693-08DB304F8CE4"));

    //    Assert.Null(messages);
    //}

    //[Fact]
    //public async Task SendMessageTest() {
    //    var sendMessageDto = new SendMessageDto {
    //        Text = "Test Send Message",
    //        ChatId = Guid.Parse("e35e97ee-ff35-4006-ec24-08db2e11bcc8"),
    //        SenderName = "User",
    //    };

    //    await _messageService.SendMessageAsync(sendMessageDto);

    //    var message = await _dataContext.Messages
    //                    .Include(m => m.Chat)
    //                    .Where(m => m.Text == sendMessageDto.Text && m.Chat.Id == sendMessageDto.ChatId)
    //                    .FirstOrDefaultAsync();

    //    Assert.NotNull(message);
    //}

    //[Fact]
    //public async Task DeleteTestAsync() {
    //    var id = Guid.Parse("ed61d56a-7ee7-41d6-7c27-08db2ef590d3");

    //    await _messageService.DeleteMessageByIdAsync(id);

    //    var message = await _dataContext.Messages.FindAsync(id);

    //    Assert.Null(message);
    //}

    //[Fact]
    //public async Task UpdateTestAsync() {
    //    var id = Guid.Parse("ffb90352-4dfb-4f37-c691-08db304f8ce4");

    //    var updateMessageDto = new UpdateMessageDto {
    //        Id = id,
    //        Text = "Update Test",
    //    };

    //    await _messageService.UpdateMessageByIdAsync(updateMessageDto);

    //    var message = await _dataContext.Messages.FindAsync(updateMessageDto.Id);

    //    Assert.NotNull(message);

    //    Assert.Equal(message.Text, updateMessageDto.Text);
    //}
}
