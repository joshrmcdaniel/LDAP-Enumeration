using System;
using System.Net;
using CommandLine;
using System.Text.RegularExpressions;

namespace LDAP_Enumeration
{
    /**
     * Author: Josh Mcdaniel
     * 
     * The purpose of LDAP-Enumeration is to run queries against a windows LDAP server
     */
    // Command line options parser
    public class Options
    {
        [Option('p', "port", Required=false, Default=389, HelpText="Port to connect to (default: 389)")]
        public int Port { get; set; }
        
        [Option('h', "host", Required=false, Default="localhost", HelpText="Host to connect to (default: 'localhost')")]
        public String Host { get; set; }

        [Option('q', "query", Required=false, Default= "objectclass=*", HelpText="Query to run (default: query all")]
        public String Query { get; set; }

        [Option('v', "verbose", Required=false, HelpText="Show debug text")]
        public bool Verbose { get; set; }
    }
    class Program
    {
        public static String Host;
        public static String Query;
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
                    throw new ArgumentOutOfRangeException("Port must be in 1-65535");
              
                Port = (ushort) o.Port;
              
                if(!Regex.IsMatch(o.Host, @"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)")
                && !Regex.IsMatch(o.Host, @"^(ldap://)?[A-Za-z0-9]+\.[A-Za-z0-9]+$"))
                    {
                    throw new ArgumentException("Invalid host");
                    }

                Host = o.Host;
                Query = o.Query;
                

            });
        }
    }
}
