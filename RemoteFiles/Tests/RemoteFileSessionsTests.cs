using Odin.RemoteFiles;
using Odin.System;


namespace Tests.Odin.RemoteFiles;

public class RemoteFileSessionsTests
{
    [Theory(Skip = "uses local sftp server...")]
    [InlineData("Sandulela_Daily_Electricity_Transaction_Recon_Flash_client_2023-08-13.csv", 1)]
    [InlineData("Sandulela_Daily_Electricity_Transaction_Recon_Flash_??????_2023-08-13.csv", 1)]
    [InlineData("Sandulela_Daily_Electricity_Transaction_*_2023-08-13.csv", 1)]
    [InlineData(null, -1)]
    [InlineData("Sandulela_Daily_Electricity_Transaction_*.csv", -1)]
    public void GetFiles_gets_all_files_successfully(string? filePath, int expectedCount = -1)
    {
        const string baseDirectory = "/Users/matthewderman/Code/Flash/SFTP/Sandulela/";
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                { "local", $"Protocol=sftp;Host=mattbook.local;Port=22;UserName=Matthew Derman;Password=CHANGE"}
            }
        };
        RemoteFileSessionFactory factory = new RemoteFileSessionFactory(remoteFileConfig);
        ResultValue<IRemoteFileSession> sut = factory.CreateRemoteFileSession("local");
        IEnumerable<IRemoteFileInfo> results = sut.Value!.GetFiles(baseDirectory ,filePath);
        if (expectedCount != -1)
        {
            Assert.Equal(expectedCount, results.Count());
        }
        else
        {
            Assert.True(results.Count() > 1);
        }
    }
    
}
