using System;
using System.Threading.Tasks;
using System.Diagnostics;
using NUnit.Framework;

namespace Client.Tests {

    [TestFixture]
    public class ClientTest {
        public static readonly string[] servers = {
            "185.64.116.15",
            "2azzarita.hopto.org",
            "4cii7ryno5j3axe4.onion",
            "4yi77lkjgy4bwtj3.onion",
            "52.1.56.181",
            "btc.jochen-hoenicke.de",
            "dedi.jochen-hoenicke.de",
            "electrum.coineuskal.com",
            "electrum.petrkr.net",
            "electrum.vom-stausee.de",
            "electrumx-core.1209k.com",
            "ip101.ip-54-37-91.eu",
            "ip119.ip-54-37-91.eu",
            "ip120.ip-54-37-91.eu",
            "ip239.ip-54-36-234.eu",
            "j5jfrdthqt5g25xz.onion",
            "kciybn4d4vuqvobdl2kdp3r2rudqbqvsymqwg4jomzft6m6gaibaf6yd.onion",
            "kirsche.emzy.de",
            "n3o2hpi5xnf3o356.onion",
            "ndndword5lpb7eex.onion",
            "ozahtqwp25chjdjd.onion",
            "sslnjjhnmwllysv4.onion",
            "wofkszvyz7mhn3bb.onion",
            "xray587.startdedicated.de",
            "y4td57fxytoo5ki7.onion",
        };

        private async Task LoopThroughElectrumServers (Func<TcpEcho.StratumClient,Task> action) {
            var successfulCount = 0;
            Console.WriteLine();
            for (int i = 0; i < servers.Length; i++) {
                Console.Write($"Trying to query '{servers[i]}'... ");
                try {
                    var client = new TcpEcho.StratumClient (servers[i], 50001);
                    await action(client);
                    Console.WriteLine("success");
                    successfulCount++;
                } catch (TcpEcho.CommunicationUnsuccessfulException error) {
                    Console.Error.WriteLine ("failure");
                }
                catch (AggregateException aggEx)
                {
                    if (!(aggEx.InnerException is TcpEcho.CommunicationUnsuccessfulException))
                        throw;
                    Console.Error.WriteLine ("failure");
                }
            }
            Assert.That (successfulCount, Is.GreaterThan(1));
        }

        [Test]
        public async Task ConnectWithElectrumServersTransactionGet () {
            await LoopThroughElectrumServers(async client => {
                var result = await client.BlockchainTransactionGet (
                    17,
                    "2f309ef555110ab4e9c920faa2d43e64f195aa027e80ec28e1d243bd8929a2fc"
                );
                Assert.That(result, Is.Not.Null);
            });
        }

        [Test]
        public async Task ConnectWithElectrumServersEstimateFee () {
            await LoopThroughElectrumServers(async client => {
                var result = await client.BlockchainEstimateFee (17, 6);
                Assert.That(result, Is.Not.Null);
            });
        }

        [Test]
        public async Task ProperNonEternalTimeout()
        {
            var someRandomIP = "52.1.57.181";
            bool? succesful = null;

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            try
            {
                var client = new TcpEcho.StratumClient(someRandomIP, 50001);
                var result = await client.BlockchainTransactionGet(
                    17,
                    "2f309ef555110ab4e9c920faa2d43e64f195aa027e80ec28e1d243bd8929a2fc"
                );
                succesful = true;
            }
            catch
            {
                succesful = false;
                stopWatch.Stop();
            }
            Assert.That(succesful.HasValue, Is.EqualTo(true), "test is broken?");
            Assert.That(succesful.Value, Is.EqualTo(false), "IP is not too random? port was open actually!");
            Assert.That(stopWatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(2)));
        }
    }
}
