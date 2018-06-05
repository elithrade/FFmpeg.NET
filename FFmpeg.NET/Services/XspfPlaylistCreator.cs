﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace FFmpeg.NET.Services
{
    public class XspfPlaylistCreator : IPlaylistCreator
    {
        public string Create(IDictionary<FileInfo, MetaData> files)
        {
            var playlist = new Playlist
            {
                Title = "Playlist",
                Version = 1,
                TrackList = new Track[files.Count],
                Extension = new PlaylistExtension
                {
                    Application = "http://www.videolan.org/vlc/playlist/0",
                    Items = new Item[files.Count]
                }
            };

            var index = 0;
            foreach (var file in files)
            {
                playlist.TrackList[index] =
                    new Track
                    {
                        Title = file.Key.Name,
                        Duration = (uint) file.Value.Duration.TotalMilliseconds,
                        Location = $"file:///{file.Key.FullName.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)}",
                        Extension = new Extension
                        {
                            Application = "http://www.videolan.org/vlc/playlist/0",
                            Id = index
                        }
                    };
                playlist.Extension.Items[index] = new Item {TId = index};
                index++;
            }

            using (var sw = new Utf8StringWriter())
            using (var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings {Indent = true, Encoding = Encoding.UTF8}))
            {
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "http://xspf.org/ns/0/");
                namespaces.Add("vlc", "http://www.videolan.org/vlc/playlist/ns/0/");
                new XmlSerializer(typeof(Playlist)).Serialize(xmlWriter, playlist, namespaces);
                return sw.ToString();
            }
        }
    }


    internal sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    [XmlRoot("playlist")]
    public class Playlist
    {
        [XmlAttribute("version")]
        public int Version { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlArray("trackList")]
        [XmlArrayItem("track")]
        public Track[] TrackList { get; set; }

        [XmlElement("extension")]
        public PlaylistExtension Extension { get; set; }
    }

    public class Track
    {
        [XmlElement("location")]
        public string Location { get; set; }

        [XmlElement("title")]
        public string Title { get; set; }

        [XmlElement("duration")]
        public long Duration { get; set; }

        [XmlElement("extension")]
        public Extension Extension { get; set; }
    }

    public class Extension
    {
        [XmlAttribute("application")]
        public string Application { get; set; }

        [XmlElement("id", Namespace = "http://www.videolan.org/vlc/playlist/ns/0/")]
        public int Id { get; set; }
    }

    public class PlaylistExtension
    {
        [XmlAttribute("application")]
        public string Application { get; set; }

        [XmlArrayItem("item", Namespace = "http://www.videolan.org/vlc/playlist/ns/0/")]
        public Item[] Items { get; set; }
    }

    public class Item
    {
        [XmlAttribute("tid")]
        public int TId { get; set; }
    }
}