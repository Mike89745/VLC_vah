using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace vlc
{
	public class Video
	{
        public string path { get; set; }
        public bool isURL { get; set; }
        public double videoLength { get; set; }
        public string FileName { get; set; }
        public Video(string VideoPath,bool isUrl)
        {
            path = VideoPath;
            isURL = isUrl;
            FileName = Path.GetFileNameWithoutExtension(VideoPath);
        }
     
	}
}