// See https://aka.ms/new-console-temp
using System.Text;
using RC4.Coder;

string key = "SimpleKey";

ICoder coder = new DesCoder();
coder.CodeFile(
    Encoding.ASCII.GetBytes(key), 
    "C:\\MyProjects\\DotNet\\defense\\RC4_Allgho\\RC4\\codingResource.txt", 
    "C:\\MyProjects\\DotNet\\defense\\RC4_Allgho\\RC4\\result.txt", 
    Mode.Encode
    );
coder.CodeFile( 
    Encoding.ASCII.GetBytes(key), 
    "C:\\MyProjects\\DotNet\\defense\\RC4_Allgho\\RC4\\result.txt", 
    "C:\\MyProjects\\DotNet\\defense\\RC4_Allgho\\RC4\\codingResource2.txt", 
    Mode.Decode
);

    Sha1 sha = new Sha1();
    sha.Hash(
        "C:\\MyProjects\\DotNet\\defense\\RC4_Allgho\\RC4\\codingResource.txt", 
        "C:\\MyProjects\\DotNet\\defense\\RC4_Allgho\\RC4\\hash.txt" 
        );