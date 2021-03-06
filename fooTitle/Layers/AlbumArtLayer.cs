/*
    Copyright 2005 - 2006 Roman Plasil
	http://foo-title.sourceforge.net
    This file is part of foo_title.

    foo_title is free software; you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation; either version 2.1 of the License, or
    (at your option) any later version.

    foo_title is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with foo_title; if not, write to the Free Software
    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Xml;
using fooManagedWrapper;

namespace fooTitle.Layers {
    [LayerTypeAttribute("album-art")]
    class AlbumArtLayer : Layer {

        protected Bitmap albumArt;
        protected Bitmap noCover;
        /// <summary>
        /// This is a scaled copy of albumArt. This prevents rescaling
        /// a large bitmap every frame.
        /// </summary>
        protected Bitmap cachedResized;

        /// <summary>
        /// If an album art exists, returns the image of the album art. If it does
        /// not exist, returns the no cover image. If there is a cached version of
        /// the album art of the correct size, returns that.
        /// </summary>
        protected Bitmap currentImage {
            get {
                if (albumArt != null) {
                    
                    if (cachedResized != null) {
                        if ((ClientRect.Width == cachedResized.Width) && (ClientRect.Height == cachedResized.Height))
                            return cachedResized;
                        else
                            return albumArt;
                    } else {
                        return albumArt;
                    }

                } else {
                    return noCover;
                }

            }
        }

        public AlbumArtLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node) {
            try {
                XmlNode contents = GetFirstChildByName(node, "contents");
                XmlNode NoAlbumArt = GetFirstChildByName(contents, "NoAlbumArt");
                string name = GetNodeValue(NoAlbumArt);
                noCover = Main.GetInstance().CurrentSkin.GetSkinImage(name);
            } catch (Exception) {
                noCover = null;
            }

            Main.GetInstance().CurrentSkin.OnPlaybackNewTrackEvent += new OnPlaybackNewTrackDelegate(CurrentSkin_OnPlaybackNewTrackEvent);
        }

        protected string findAlbumArt(CMetaDBHandle song, string filenames) {
            String songPath = song.GetPath();
            // remove this prefix
            if (songPath.StartsWith("file:\\"))
                songPath = songPath.Substring(6);
            else if (songPath.StartsWith("file://"))
                songPath = songPath.Substring(7);

            string[] extensions = new string[] { ".jpg", ".png", ".bmp", ".gif", ".jpeg" };
            string[] splitFiles = filenames.Split(new char[] { ';' });
            string[] formattedFiles = new string[splitFiles.Length];
            int i = 0;
            foreach (string f in splitFiles) {
                formattedFiles[i] = Main.PlayControl.FormatTitle(song, f);
                i++;
            }

            foreach (string f in formattedFiles) {
                foreach (string ext in extensions) {
                    String path = Path.Combine(Path.GetDirectoryName(songPath), f + ext);
                    if (File.Exists(path))
                        return path;
                }
            }

            return null;
        }

        void CurrentSkin_OnPlaybackNewTrackEvent(fooManagedWrapper.CMetaDBHandle song) {
            cachedResized = null;
            string fileName = findAlbumArt(song, Main.GetInstance().AlbumArtFilenames.Value);
            if (fileName != null) {
                try {
                    Bitmap tmp = new Bitmap(fileName);
                    albumArt = new Bitmap(tmp);
                    tmp.Dispose();
                } catch (Exception e) {
                    albumArt = null;
                    fooManagedWrapper.CConsole.Warning(String.Format("Cannot open album art {0} : {1}", fileName, e.Message));
                }
            } else {
                albumArt = null;
            }
        }

		protected override void drawImpl() {
            prepareCachedImage();
            Bitmap toDraw = this.currentImage;

            if (toDraw != null) {
                Display.Canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                Display.Canvas.DrawImage(toDraw, ClientRect.X, ClientRect.Y, ClientRect.Width, ClientRect.Height);
            }
		}

        private void prepareCachedImage() {
            if (albumArt == null)
                return;

            if ((cachedResized != null) && (cachedResized.Width == ClientRect.Width) && (cachedResized.Height == ClientRect.Height))
                return;

            if ((ClientRect.Width <= 0) || (ClientRect.Height <= 0))
                return;

            cachedResized = new Bitmap(ClientRect.Width, ClientRect.Height);
            using (Graphics canvas = Graphics.FromImage(cachedResized)) {
                canvas.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                canvas.DrawImage(albumArt, 0, 0, ClientRect.Width, ClientRect.Height);
            }

        }

    }
}
