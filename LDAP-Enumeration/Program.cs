using System;
using System.Net;
using CommandLine;
using System.Text.RegularExpressions;
using System.DirectoryServices;

namespace LDAP_Enumeration
{
    /**
     * Author: [REDACTED]
     * 
     * The purpose of LDAP-Enumeration is to run queries against a windows LDAP server
     */
    // Command line options parser
    public class Options
    {
        [Option('P', "port", Required=false, Default=389, HelpText="Port to connect to (default: 389)")]
        public int Port { get; set; }
        
        [Option('h', "host", Required=false, Default=null, HelpText="Host to connect to (default: 'localhost')")]
        public String Host { get; set; }

        [Option('q', "query", Required=false, Default= "objectclass=*", HelpText="Query to run (default: query all")]
        public String Query { get; set; }

        [Option('u', "username", Required=false, Default=null, HelpText="Username to authenticate as (default: \"\")")]
        public String Username { get; set; }

        [Option('P', "password", Required=false, Default=null, HelpText ="Password to authenticate with (defuault:\"\"")]
        public String Password { get; set; }

        [Option('v', "verbose", Required=false, HelpText="Show debug text")]
        public bool Verbose { get; set; }
    }
    class Program
    {
        public static String Host;
        public static String Query;
        public static String Username;
        public static String Password;
        public static ushort Port;
        public static bool Verbose;

        static void Main(string[] args)
        {

            Parser.Default.ParseArguments<Options>(args).WithParsed<Options>(o =>
            {
                Verbose = o.Verbose;
                if (Verbose)
                    Console.WriteLine("Parsing arguments");
                if(o.Port < 1 || o.Port > 65535)
                    throw new ArgumentOutOfRangeException("Port must be 1-65535");
              
                Port = (ushort) o.Port;
                if (o.Host == null)
                    o.Host = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName;
                if (!Regex.IsMatch(o.Host, @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)")
                && !Regex.IsMatch(o.Host, @"^(ldap://)?[A-Za-z0-9]+\.[A-Za-z0-9]+$"))
                    {
                    throw new ArgumentException($"Invalid host: {o.Host}");
                    }

                Host = o.Host.IndexOf("ldap://") == -1 ? "ldap://" + o.Host : o.Host;
                Query = o.Query;
                Username = o.Username;
                Password = o.Password;

                if (Verbose)
                    Console.WriteLine("Arguments successfully parsed");
                RunQuery();
            });
        }

        private static void RunQuery()
        {
            if (Verbose)
                Console.WriteLine($"Running query \"{Query}\"");
      
            DirectoryEntry LDAPConnection;
            if (Username == null)
                LDAPConnection = new DirectoryEntry(Host);
            else
                LDAPConnection = new DirectoryEntry(Host, Username, Password);
            LDAPConnection.Path = new Regex(@"\.").Replace(Host, "dc=");
            
            if (Verbose)
                Console.Write($"LDAPConnection.Path = {LDAPConnection.Path}");
            
            DirectorySearcher Search = new DirectorySearcher(LDAPConnection);
            Search.Filter = Query;

            using (SearchResultCollection Results = Search.FindAll())
            {
                if (Results != null)
                {
                    Console.WriteLine("***Results***");
                    foreach (SearchResult Result in Results)
                    {
                        DirectoryEntry MyDirectoryEntry = new DirectoryEntry(Result);
                        PropertyCollection Properties = MyDirectoryEntry.Properties;

                        Console.WriteLine($"Username:\t{MyDirectoryEntry.Username}");
                        
                        foreach(String key in Properties.PropertyNames)
                            foreach(Object Collection in Properties[key])
                            {
                                Console.WriteLine($"{key}\t{Properties[key]}");
                            }
                        Console.WriteLine('\n');
                    }
                }
                else
                {
                    Console.WriteLine("No results found. Check your query");
                }
            }
        }

    }
}
