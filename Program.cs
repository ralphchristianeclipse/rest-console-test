using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;

namespace rest
{
  [DataContract]
  public class Post
  {
    [DataMember(Name = "id")]
    public int Id { get; set; }

    [DataMember(Name = "title")]
    public string Title { get; set; }

    [DataMember(Name = "body")]
    public string Body { get; set; }

    public User Author { get; set; }

    public override string ToString()
    {
      return $"\n #{Id} {Title} | {Author.Username}";
    }
  }

  [DataContract]
  public class User
  {
    [DataMember(Name = "id")]
    public int Id { get; set; }
    [DataMember(Name = "username")]
    public string Username { get; set; }

    public List<Post> posts { get; set; }

    public async Task<List<Post>> GetPosts()
    {
      Console.WriteLine(Id);
      posts = await Program.GetJson<Post>($"/posts?userId={Id}");
      posts = posts.Select(post =>
      {
        post.Author = this;
        return post;
      }).ToList();
      return posts;
    }

    public override string ToString()
    {
      var result = $"{Id} {Username}";
      posts.ForEach(post => result += post);
      return result;
    }

    public static async Task<List<User>> GetUsers()
    {
      var result = await Program.GetJson<User>("/users");
      var users = await Task.WhenAll(result.Select(async user =>
      {
        await user.GetPosts();
        return user;
      }));

      return users.ToList();
    }
  }
  class Program
  {
    static readonly HttpClient client = new HttpClient() { BaseAddress = new Uri("https://jsonplaceholder.typicode.com") };
    static void Main(string[] args)
    {
      var users = User.GetUsers().Result;
      users.ForEach(Console.WriteLine);
    }

    public static async Task<List<T>> SerializeStream<T>(Task<Stream> stream)
    {
      var serializer = new DataContractJsonSerializer(typeof(List<T>));
      var result = serializer.ReadObject(await stream) as List<T>;
      return result;
    }

    public static async Task<List<T>> GetJson<T>(string url)
    {
      var task = client.GetStreamAsync(url);
      var result = await SerializeStream<T>(task);
      return result;
    }

    public static async Task<List<T>> GetJsonSerialized<T>(string url)
    {
      var task = await client.GetStringAsync(url);
      var result = JsonConvert.DeserializeObject<List<T>>(task);
      return result;
    }

  }
}
