using DHT;

using var filter = new BootstrapFilter(TransferCenter.Instance.ValidBootstraps);
Utils.CreateDHT();
TransferCenter.Instance.DealMessage();