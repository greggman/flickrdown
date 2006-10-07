﻿using System;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace FlickrNet
{
    /// <summary>
    /// Detailed information returned by <see cref="Flickr.PhotosGetInfo(string)"/> or <see cref="Flickr.PhotosGetInfo(string, string)"/> methods.
    /// </summary>
    [System.Serializable]
    public class PhotoInfo
    {
        /// <summary>
        /// The id of the photo.
        /// </summary>
        [XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
        public string PhotoId;

        /// <summary>
        /// The secret of the photo. Used to calculate the URL (amongst other things).
        /// </summary>
        [XmlAttribute("secret", Form=XmlSchemaForm.Unqualified)]
        public string Secret;

        /// <summary>
        /// The original format of the photo. Used to calculate the URL (amongst other things).
        /// </summary>
        [XmlAttribute("originalformat", Form=XmlSchemaForm.Unqualified)]
        public string OriginalFormat;

        /// <summary>
        /// The server on which the photo resides.
        /// </summary>
        [XmlAttribute("server", Form=XmlSchemaForm.Unqualified)]
        public int Server;

        /// <summary>
        /// The date the photo was uploaded (or 'posted').
        /// </summary>
        [XmlIgnore()]
        public DateTime DateUploaded
        {
            get { return Utils.UnixTimestampToDate(dateuploaded_raw); }
        }

        /// <summary>
        /// The raw value for when the photo was uploaded.
        /// </summary>
        [XmlAttribute("dateuploaded", Form=XmlSchemaForm.Unqualified)]
        public string dateuploaded_raw;

        /// <summary>
        /// Is the photo a favourite of the current authorised user.
        /// Will be 0 if the user is not authorised.
        /// </summary>
        [XmlAttribute("isfavorite", Form=XmlSchemaForm.Unqualified)]
        public int IsFavourite;

        /// <summary>
        /// The license of the photo.
        /// </summary>
        [XmlAttribute("license", Form=XmlSchemaForm.Unqualified)]
        public int License;

        /// <summary>
        /// The owner of the photo.
        /// </summary>
        /// <remarks>
        /// See <see cref="PhotoInfoOwner"/> for more details.
        /// </remarks>
        [XmlElement("owner", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoOwner Owner;

        /// <summary>
        /// The title of the photo.
        /// </summary>
        [XmlElement("title", Form=XmlSchemaForm.Unqualified)]
        public string Title;

        /// <summary>
        /// The description of the photo.
        /// </summary>
        [XmlElement("description", Form=XmlSchemaForm.Unqualified)]
        public string Description;

        /// <summary>
        /// The visibility of the photo.
        /// </summary>
        /// <remarks>
        /// See <see cref="PhotoInfoVisibility"/> for more details.
        /// </remarks>
        [XmlElement("visibility", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoVisibility Visibility;

        /// <summary>
        /// The permissions of the photo.
        /// </summary>
        /// <remarks>
        /// See <see cref="PhotoInfoPermissions"/> for more details.
        /// </remarks>
        [XmlElement("permissions", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoPermissions Permissions;

        /// <summary>
        /// The editability of the photo.
        /// </summary>
        /// <remarks>
        /// See <see cref="PhotoInfoEditability"/> for more details.
        /// </remarks>
        [XmlElement("editability", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoEditability Editability;

        /// <summary>
        /// The number of comments the photo has.
        /// </summary>
        [XmlElement("comments", Form=XmlSchemaForm.Unqualified)]
        public int CommentsCount;

        /// <summary>
        /// The notes for the photo.
        /// </summary>
        [XmlElement("notes", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoNotes Notes;

        /// <summary>
        /// The tags for the photo.
        /// </summary>
        [XmlElement("tags", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoTags Tags;

        /// <summary>
        /// The EXIF tags for the photo.
        /// </summary>
        [XmlElement("exif", Form=XmlSchemaForm.Unqualified)]
        public ExifTag[] ExifTagCollection;

        /// <summary>
        /// The dates (uploaded and taken dates) for the photo.
        /// </summary>
        [XmlElement("dates", Form=XmlSchemaForm.Unqualified)]
        public PhotoDates Dates;

        /// <summary>
        /// The location information of this photo, if available.
        /// </summary>
        /// <remarks>
        /// Will be null if the photo has no location information stored on Flickr.
        /// </remarks>
        [XmlElement("location", Form=XmlSchemaForm.Unqualified)]
        public PhotoLocation Location;

        /// <summary>
        /// The Web url for flickr web page for this photo.
        /// </summary>
        [XmlIgnore()]
        public string WebUrl
        {
            get { return string.Format("http://www.flickr.com/photos/{0}/{1}/", Owner.UserId, PhotoId); }
        }

        private const string photoUrl = "http://static.flickr.com/{0}/{1}_{2}{3}.{4}";

        /// <summary>
        /// The URL for the square thumbnail for the photo.
        /// </summary>
        [XmlIgnore()]
        public string SquareThumbnailUrl
        {
            get { return string.Format(photoUrl, Server, PhotoId, Secret, "_s", "jpg"); }
        }

        /// <summary>
        /// The URL for the thumbnail for the photo.
        /// </summary>
        [XmlIgnore()]
        public string ThumbnailUrl
        {
            get { return string.Format(photoUrl, Server, PhotoId, Secret, "_t", "jpg"); }
        }

        /// <summary>
        /// The URL for the small version of this photo.
        /// </summary>
        [XmlIgnore()]
        public string SmallUrl
        {
            get { return string.Format(photoUrl, Server, PhotoId, Secret, "_m", "jpg"); }
        }

        /// <summary>
        /// The URL for the medium version of this photo.
        /// </summary>
        /// <remarks>
        /// There is no guarentee that this size of the image actually exists.
        /// Use <see cref="Flickr.PhotosGetSizes"/> to get a list of existing photo URLs.
        /// </remarks>
        [XmlIgnore()]
        public string MediumUrl
        {
            get { return string.Format(photoUrl, Server, PhotoId, Secret, "", "jpg"); }
        }

        /// <summary>
        /// The URL for the large version of this photo.
        /// </summary>
        /// <remarks>
        /// There is no guarentee that this size of the image actually exists.
        /// Use <see cref="Flickr.PhotosGetSizes"/> to get a list of existing photo URLs.
        /// </remarks>
        [XmlIgnore()]
        public string LargeUrl
        {
            get { return string.Format(photoUrl, Server, PhotoId, Secret, "_b"); }
        }

        /// <summary>
        /// The URL for the large version of this photo.
        /// </summary>
        /// <remarks>
        /// There is no guarentee that this size of the image actually exists.
        /// Use <see cref="Flickr.PhotosGetSizes"/> to get a list of existing photo URLs.
        /// </remarks>
        [XmlIgnore()]
        public string OriginalUrl
        {
            get {
                if (OriginalFormat == null || OriginalFormat.Length == 0)
                {
                    return string.Format(photoUrl, Server, PhotoId, Secret, "_o", "jpg");
                }
                else
                {
                    return string.Format(photoUrl, Server, PhotoId, Secret, "_o", OriginalFormat);
                }
            }
        }
    }

    /// <summary>
    /// The information about the owner of a photo.
    /// </summary>
    [System.Serializable]
    public class PhotoInfoOwner
    {
        /// <summary>
        /// The id of the own of the photo.
        /// </summary>
        [XmlAttribute("nsid", Form=XmlSchemaForm.Unqualified)]
        public string UserId;

        /// <summary>
        /// The username of the owner of the photo.
        /// </summary>
        [XmlAttribute("username", Form=XmlSchemaForm.Unqualified)]
        public string UserName;

        /// <summary>
        /// The real name (as stored on Flickr) of the owner of the photo.
        /// </summary>
        [XmlAttribute("realname", Form=XmlSchemaForm.Unqualified)]
        public string RealName;

        /// <summary>
        /// The location (as stored on Flickr) of the owner of the photo.
        /// </summary>
        [XmlAttribute("location", Form=XmlSchemaForm.Unqualified)]
        public string Location;
    }

    /// <summary>
    /// The visibility of the photo.
    /// </summary>
    [System.Serializable]
    public class PhotoInfoVisibility
    {
        /// <summary>
        /// Is the photo visible to the public.
        /// </summary>
        [XmlAttribute("ispublic", Form=XmlSchemaForm.Unqualified)]
        public int IsPublic;

        /// <summary>
        /// Is the photo visible to contacts marked as friends.
        /// </summary>
        [XmlAttribute("isfriend", Form=XmlSchemaForm.Unqualified)]
        public int IsFriend;

        /// <summary>
        /// Is the photo visible to contacts marked as family.
        /// </summary>
        [XmlAttribute("isfamily", Form=XmlSchemaForm.Unqualified)]
        public int IsFamily;
    }

    /// <summary>
    /// Who has permissions to add information to this photo (comments, tag and notes).
    /// </summary>
    [System.Serializable]
    public class PhotoInfoPermissions
    {
        /// <summary>
        /// Who has permissions to add comments to this photo.
        /// </summary>
        [XmlAttribute("permcomment", Form=XmlSchemaForm.Unqualified)]
        public PermissionComment PermissionComment;

        /// <summary>
        /// Who has permissions to add meta data (tags and notes) to this photo.
        /// </summary>
        [XmlAttribute("permaddmeta", Form=XmlSchemaForm.Unqualified)]
        public PermissionAddMeta PermissionAddMeta;
    }

    /// <summary>
    /// Information about who can edit the details of a photo.
    /// </summary>
    [System.Serializable]
    public class PhotoInfoEditability
    {
        /// <summary>
        /// Can the authorized user add new comments.
        /// </summary>
        /// <remarks>
        /// "1" = true, "0" = false.
        /// </remarks>
        [XmlAttribute("cancomment", Form=XmlSchemaForm.Unqualified)]
        public string CanComment;

        /// <summary>
        /// Can the authorized user add new meta data (tags and notes).
        /// </summary>
        /// <remarks>
        /// "1" = true, "0" = false.
        /// </remarks>
        [XmlAttribute("canaddmeta", Form=XmlSchemaForm.Unqualified)]
        public string CanAddMeta;
    }

    /// <summary>
    /// A class containing information about the notes for a photo.
    /// </summary>
    [System.Serializable]
    public class PhotoInfoNotes
    {
        /// <summary>
        /// A collection of notes for this photo.
        /// </summary>
        [XmlElement("note", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoNote[] NoteCollection;
    }

    /// <summary>
    /// A class containing information about a note on a photo.
    /// </summary>
    [System.Serializable]
    public class PhotoInfoNote
    {
        /// <summary>
        /// The notes unique ID.
        /// </summary>
        [XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
        public string NoteId;

        /// <summary>
        /// The User ID of the user who wrote the note.
        /// </summary>
        [XmlAttribute("author", Form=XmlSchemaForm.Unqualified)]
        public string AuthorId;

        /// <summary>
        /// The name of the user who wrote the note.
        /// </summary>
        [XmlAttribute("authorname", Form=XmlSchemaForm.Unqualified)]
        public string AuthorName;

        /// <summary>
        /// The x (left) position of the top left corner of the note.
        /// </summary>
        [XmlAttribute("x", Form=XmlSchemaForm.Unqualified)]
        public int XPosition;

        /// <summary>
        /// The y (top) position of the top left corner of the note.
        /// </summary>
        [XmlAttribute("y", Form=XmlSchemaForm.Unqualified)]
        public int YPosition;

        /// <summary>
        /// The width of the note.
        /// </summary>
        [XmlAttribute("w", Form=XmlSchemaForm.Unqualified)]
        public int Width;

        /// <summary>
        /// The height of the note.
        /// </summary>
        [XmlAttribute("h", Form=XmlSchemaForm.Unqualified)]
        public int Height;

        /// <summary>
        /// The text of the note.
        /// </summary>
        [XmlText()]
        public string NoteText;
    }

    /// <summary>
    /// A class containing a collection of tags for the photo.
    /// </summary>
    [System.Serializable]
    public class PhotoInfoTags
    {
        /// <summary>
        /// A collection of tags for the photo.
        /// </summary>
        [XmlElement("tag", Form=XmlSchemaForm.Unqualified)]
        public PhotoInfoTag[] TagCollection;
    }

    /// <summary>
    /// The details of a tag of a photo.
    /// </summary>
    [System.Serializable]
    public class PhotoInfoTag
    {
        /// <summary>
        /// The id of the tag.
        /// </summary>
        [XmlAttribute("id", Form=XmlSchemaForm.Unqualified)]
        public string TagId;

        /// <summary>
        /// The author id of the tag.
        /// </summary>
        [XmlAttribute("author", Form=XmlSchemaForm.Unqualified)]
        public string AuthorId;

        /// <summary>
        /// Author of the tag - only available if using <see cref="Flickr.TagsGetListPhoto"/>.
        /// </summary>
        [XmlAttribute("authorname", Form=XmlSchemaForm.Unqualified)]
        public string AuthorName;

        /// <summary>
        /// Raw copy of the tag, as the user entered it.
        /// </summary>
        [XmlAttribute("raw", Form=XmlSchemaForm.Unqualified)]
        public string Raw;

        /// <summary>
        /// The actually tag.
        /// </summary>
        [XmlText()]
        public string TagText;
    }

}