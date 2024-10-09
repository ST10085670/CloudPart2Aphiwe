using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using System.Threading.Tasks;

namespace POE_Model_Library
{
    public class QueueService
    {
        private readonly QueueClient _queueClient;

        public QueueService(string connectionString, string queueName)
        {
            _queueClient = new QueueClient(connectionString, queueName);
            _queueClient.CreateIfNotExists(); // Ensure the queue exists
        }

        public async Task SendMessage(string message)
        {
            await _queueClient.SendMessageAsync(message);
        }

        /// <summary>
        /// Reads a message from the queue and deletes it.
        /// </summary>
        public async Task<string> ReadMessageAsync()
        {
            // Receive a message from the queue
            QueueMessage[] receivedMessages = await _queueClient.ReceiveMessagesAsync(maxMessages: 1);

            // Check if any messages were received
            if (receivedMessages.Length == 0)
            {
                return null; // No messages in the queue
            }

            // Get the first message
            QueueMessage message = receivedMessages[0];

            // Delete the message after processing it
            await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);

            return message.MessageText; // Return the message text
        }
    }
}
