using MCTG_Brian.Server;


public class Program
{ 
    // Main entry point for the program
    public static void Main(string[] args)
    {     
        Server server = new Server("127.0.0.1", 10001);
        
        server.Start();

        server.Stop();   // will be never reached    
    }
}
