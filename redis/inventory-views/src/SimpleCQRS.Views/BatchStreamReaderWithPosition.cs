//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;
//using StackExchange.Redis;

//namespace SimpleCQRS.Views
//{
//    public class BatchStreamReaderWithPosition
//    {
//        readonly Func<IDatabase> connectionFact;
//        readonly ILogger logger;
//        readonly ILoggerFactory loggerFactory;
//        readonly Func<ResolvedEvent[], Task> processEvents;
//        readonly int batchSize;
//        readonly TimeSpan interval = TimeSpan.FromSeconds(15);
//        readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

//        readonly ICheckPointWriter checkPointWriter;
//        readonly string streamName;


//        public BatchStreamReaderWithPosition(ILoggerFactory logFact,
//            string streamName,
//            IDatabase connection,
//            ICheckPointWriter checkPointWriter,
//            Func<ResolvedEvent[], Task> processEvents, int batchSize = 120)
//        {
//            this.logger = logFact.CreateLogger<BatchStreamReaderWithPosition>();
//            this.loggerFactory = logFact;
//            this.connectionFact = connection;
//            this.processEvents = processEvent;
//            this.batchSize = batchSize;
//            this.checkPointWriter = checkPointWriter;
//            this.streamName = streamName;
//        }

//        public async Task StartAndBlockTillGetConnection()
//        {
//            try
//            {
//                connectionFact.BlockTillGetConnection();
//                logger.LogInformation("Event processor has a connection");
//                await checkPointWriter.Init();

//                logger.LogInformation($"checkpoint after init starting with {checkPointWriter.GetCheckPoint() ?? -1}");

//                Start();
//                logger.LogInformation("Event processor started");
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(new EventId(), ex, $"Error starting processor   {ex.Message} ");
//                throw;
//            }
//        }

//        public void Start()
//        {
//            var connection = connectionFact.Unwrap();
//            logger.LogInformation($"EventStore Subscription {streamName} using connection {connection.ConnectionName}");

//            ActionTimer.RepeatAsyncActionEvery(ReadFromStream, interval, cancellationTokenSource.Token, logger);

//            logger.LogInformation($"EventStore Subscription {connection.ConnectionName + streamName} started");
//        }

//        async Task ReadFromStream()
//        {
//            try
//            {
//                bool isEndOfStream = false;

//                while (isEndOfStream == false)
//                    isEndOfStream = await ProcessBatch();
//            }
//            catch (EventStore.ClientAPI.Exceptions.WrongExpectedVersionException ex)
//            {
//                logger.LogError(new EventId(), ex, $"Concurrency Exception, delaying for 1 minute then exiting {ex.Message} ");
//                await Task.Delay(TimeSpan.FromMinutes(1));
//                Environment.Exit(13);
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(new EventId(), ex, $"Exception processing batch, will continue to retry,    {ex.Message} ");
//            }
//        }

//        private async Task<bool> ProcessBatch()
//        {
//            var conn = connectionFact.Unwrap();
//            var pos = checkPointWriter.GetCheckPoint() ?? 0;

//            var result = await conn.ReadStreamEventsForwardAsync(streamName, pos, this.batchSize, true);
//            bool isEnd = result.IsEndOfStream;
//            if (isEnd && result.Events.Count() == 0)
//            {
//                logger.LogDebug($"{conn.ConnectionName} at end stream {this.streamName} position {pos}");
//            }
//            else
//            {
//                logger.LogDebug($"read Events {result.Events.Count()} from stream {this.streamName} position {pos}");
//                await processEvents(result.Events);
//                logger.LogDebug($"Processed Events from stream {this.streamName} ,connection {conn.ConnectionName},  count {result.Events.Count()}");
//                if (result.NextEventNumber >= 0)
//                {
//                    await checkPointWriter.SetCheckpoint(result.NextEventNumber);
//                    logger.LogInformation($"updated checkpoint to  {result.NextEventNumber} after processing {result.Events.Count()} from stream {this.streamName}");
//                }
//            }
//            return isEnd;
//        }

//        public void Stop()
//        {
//            cancellationTokenSource.Cancel();
//        }

//    }
//}
