using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BoxObjects
{
    public class BoxEnums
    {
        
        public enum ObjectType { FILE, FOLDER, WEB_LINK, UNSPECIFIED, COMMENT }
        public enum ContentType { NAME, DESCRIPTION, FILE_CONTENT, COMMENTS, TAGS, UNSPECIFIED }
        public enum CollaboratorRole
        {
            EDITOR, VIEWER, PREVIEWER, UPLOADER, PREVIEWER_UPLOADER,
            VIEWER_UPLOADER, CO_OWNER, OWNER
        }

        public enum SharedLinkAccess
        {
            OPEN, COMPANY, COLLABORATORS
        }

        public static string DecodeBoxObjectType(BoxEnums.ObjectType bct)
        {
            try
            {
                switch (bct)
                {
                    case BoxEnums.ObjectType.FILE:
                        return "file";
                    case BoxEnums.ObjectType.FOLDER:
                        return "folder";
                    case BoxEnums.ObjectType.WEB_LINK:
                        return "web_link";
                    case BoxEnums.ObjectType.COMMENT:
                        return "comment";
                    default:
                        return "";
                }
            }
            finally
            {

            }
        }

        public static string DecodeBoxContentType(BoxEnums.ContentType bct)
        {
            try
            {
                switch (bct)
                {
                    case BoxEnums.ContentType.NAME:
                        return "name";
                    case BoxEnums.ContentType.DESCRIPTION:
                        return "description";
                    case BoxEnums.ContentType.FILE_CONTENT:
                        return "file_content";
                    case BoxEnums.ContentType.COMMENTS:
                        return "comments";
                    case BoxEnums.ContentType.TAGS:
                        return "tags";
                    default:
                        return "";
                }
            }
            finally
            {

            }
        }

        public static string DecodeBoxCollaboratorRole(BoxEnums.CollaboratorRole bcr)
        {
            try
            {
                switch (bcr)
                {
                    case BoxEnums.CollaboratorRole.CO_OWNER:
                        return "co-owner";
                    case BoxEnums.CollaboratorRole.EDITOR:
                        return "editor";
                    case BoxEnums.CollaboratorRole.OWNER:
                        return "owner";
                    case BoxEnums.CollaboratorRole.PREVIEWER:
                        return "previewer";
                    case BoxEnums.CollaboratorRole.UPLOADER:
                        return "uploader";
                    case BoxEnums.CollaboratorRole.VIEWER:
                        return "viewer";
                    case BoxEnums.CollaboratorRole.PREVIEWER_UPLOADER:
                        return "previewer uploader";
                    case BoxEnums.CollaboratorRole.VIEWER_UPLOADER:
                        return "viewer uploader";
                    default:
                        return "viewer";
                }
            }
            finally { }
        }

        public static string DecodeBoxSharedLinkAccess(BoxEnums.SharedLinkAccess bcr)
        {
            try
            {
                switch (bcr)
                {
                    case BoxEnums.SharedLinkAccess.COLLABORATORS:
                        return "collaborators";
                    case BoxEnums.SharedLinkAccess.COMPANY:
                        return "company";
                    case BoxEnums.SharedLinkAccess.OPEN:
                        return "open";
                    default:
                        return "open";
                }
            }
            finally { }
        }

    }
    /// <summary>
    /// The classes in this file are objects which JSON.net uses to generate the JSON strings used in the payload
    /// of our requests to the Box API
    /// </summary>
    class NewFolder
    {
        private string m_name = "";
        private Parent m_parent = null;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the new folder</param>
        /// <param name="parent">Parent object of the folder</param>
        public NewFolder(string name, Parent parent)
        {
            m_name = name;
            m_parent = parent;
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        public Parent parent
        {
            get
            {
                return m_parent;
            }

            set
            {
                m_parent = value;
            }
        }
    }

    public class Parent
    {
        public Parent()
        {

        }
        public Parent(string id)
        {
            m_id = id;
        }

        private string m_id = "";
        private string m_type;
        private string m_sequence_id;
        private string m_etag;
        private string m_name;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Box ID of the parent object</param>
        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public string type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        public string sequence_id
        {
            get
            {
                return m_sequence_id;
            }

            set
            {
                m_sequence_id = value;
            }
        }

        public string etag
        {
            get
            {
                return m_etag;
            }

            set
            {
                m_etag = value;
            }
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }
    }

    public class Item_FileFolder
    {
        public Item_FileFolder()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">Use BoxEnums.DecodeBoxObjectType (file/folder/web_link)</param>
        /// <param name="id">Box Id of the Item</param>
        public Item_FileFolder(string type, string id)
        {
            m_type = type;
            m_id = id;
        }

        /// <summary>
        /// Make an item, specifying its name and its parent ID
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parentID"></param>
        public Item_FileFolder(string name, Int64 parentID)
        {
            m_name = name;
            m_parent = new Parent(parentID.ToString());
        }

        public Item_FileFolder(string name=null, string description=null, Parent parent=null,
            SharedLink sharedLink = null, string[] tags=null, BoxUser owner = null, 
            string syncState = null)
        {
            m_name = name;
            m_description = description;
            m_parent = parent;
            m_sharedLink = sharedLink;
            m_owned_by = owned_by;
            m_tags = tags;
        }
        private string m_type;
        private string m_id;
        private string m_sequence_id;
        private string m_etag;
        private string m_name;
        private string m_created_at;
        private string m_modified_at;
        private string m_description;
        private int m_size = 0;
        private PathCollection m_path_collection;
        private BoxUser m_created_by;
        private BoxUser m_modified_by;
        private BoxUser m_owned_by;
        private SharedLink m_sharedLink;
        private FolderUploadEmail m_folder_upload_email;
        private Parent m_parent;
        private string m_item_status;
        private ItemCollection m_item_collection;
        private string[] m_tags;
        private string m_version;
        public string type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public string sequence_id
        {
            get
            {
                return m_sequence_id;
            }

            set
            {
                m_sequence_id = value;
            }
        }

        public string etag
        {
            get
            {
                return m_etag;
            }

            set
            {
                m_etag = value;
            }
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        public string created_at
        {
            get
            {
                return m_created_at;
            }

            set
            {
                m_created_at = value;
            }
        }

        public string modified_at
        {
            get
            {
                return m_modified_at;
            }

            set
            {
                m_modified_at = value;
            }
        }

        public string description
        {
            get
            {
                return m_description;
            }

            set
            {
                m_description = value;
            }
        }

        public int size
        {
            get
            {
                return m_size;
            }

            set
            {
                m_size = value;
            }
        }

        public PathCollection path_collection
        {
            get
            {
                return m_path_collection;
            }

            set
            {
                m_path_collection = value;
            }
        }

        public BoxUser created_by
        {
            get
            {
                return m_created_by;
            }

            set
            {
                m_created_by = value;
            }
        }

        public BoxUser modified_by
        {
            get
            {
                return m_modified_by;
            }

            set
            {
                m_modified_by = value;
            }
        }

        public BoxUser owned_by
        {
            get
            {
                return m_owned_by;
            }

            set
            {
                m_owned_by = value;
            }
        }

        public SharedLink shared_link
        {
            get
            {
                return m_sharedLink;
            }

            set
            {
                m_sharedLink = value;
            }
        }

        public FolderUploadEmail folder_upload_email
        {
            get
            {
                return m_folder_upload_email;
            }

            set
            {
                m_folder_upload_email = value;
            }
        }

        public Parent parent
        {
            get
            {
                return m_parent;
            }

            set
            {
                m_parent = value;
            }
        }

        public string item_status
        {
            get
            {
                return m_item_status;
            }

            set
            {
                m_item_status = value;
            }
        }

        public ItemCollection item_collection
        {
            get
            {
                return m_item_collection;
            }

            set
            {
                m_item_collection = value;
            }
        }

        public string[] tags
        {
            get
            {
                return m_tags;
            }

            set
            {
                m_tags = value;
            }
        }

        public string version
        {
            get
            {
                return m_version;
            }

            set
            {
                m_version = value;
            }
        }
    }

    public class PathCollection
    {
        public PathCollection()
        {

        }
        private int m_total_count = 0;
        private Entries[] m_entries = { new Entries() };

        public int total_count
        {
            get
            {
                return m_total_count;
            }

            set
            {
                m_total_count = value;
            }
        }

        public Entries[] entries
        {
            get
            {
                return m_entries;
            }

            set
            {
                m_entries = value;
            }
        }
    }

    public class Entries
    {
        public Entries()
        {

        }
        private string m_type;
        private string m_id;
        private string m_sequence_id;
        private string m_etag;
        private string m_name;

        public string type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public string sequence_id
        {
            get
            {
                return m_sequence_id;
            }

            set
            {
                m_sequence_id = value;
            }
        }

        public string etag
        {
            get
            {
                return m_etag;
            }

            set
            {
                m_etag = value;
            }
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }
    }

    /// <summary>
    /// Use to represent individual user or group
    /// </summary>
    public class BoxUser
    {
        public BoxUser()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">User type ('user' or 'group')</param>
        /// <param name="id">User Box ID</param>
        /// <param name="login">User email</param>
        public BoxUser(string type, string id = null, string login = null)
        {
            m_type = type;
            m_id = id;
            m_login = login;
        }
        private string m_type;
        private string m_id;
        private string m_name;
        private string m_login;

        public string type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        public string login
        {
            get
            {
                return m_login;
            }

            set
            {
                m_login = value;
            }
        }
    }

    public class SharedLink
    {
        public SharedLink()
        {

        }

        public SharedLink(string access=null, string password=null, 
            string unsharedat=null, BoxPermissions permissions=null)
        {
            m_access = access;
            m_password = password;
            m_unshared_at = unsharedat;
            m_permissions = permissions;
        }
        private string m_url;
        private string m_download_url;
        private string m_vanity_url;
        private Boolean m_is_password_enabled;
        private string m_password;
        private string m_unshared_at;
        private int m_download_count = 0;
        private int m_preview_count = 0;
        private string m_access;
        private BoxPermissions m_permissions;

        public string url
        {
            get
            {
                return m_url;
            }

            set
            {
                m_url = value;
            }
        }

        public string download_url
        {
            get
            {
                return m_download_url;
            }

            set
            {
                m_download_url = value;
            }
        }

        public string vanity_url
        {
            get
            {
                return m_vanity_url;
            }

            set
            {
                m_vanity_url = value;
            }
        }

        public bool is_password_enabled
        {
            get
            {
                return m_is_password_enabled;
            }

            set
            {
                m_is_password_enabled = value;
            }
        }

        public string unshared_at
        {
            get
            {
                return m_unshared_at;
            }

            set
            {
                m_unshared_at = value;
            }
        }

        public int download_count
        {
            get
            {
                return m_download_count;
            }

            set
            {
                m_download_count = value;
            }
        }

        public int preview_count
        {
            get
            {
                return m_preview_count;
            }

            set
            {
                m_preview_count = value;
            }
        }

        public string access
        {
            get
            {
                return m_access;
            }

            set
            {
                m_access = value;
            }
        }

        public BoxPermissions Permissions
        {
            get
            {
                return m_permissions;
            }

            set
            {
                m_permissions = value;
            }
        }

        public string Password
        {
            get
            {
                return m_password;
            }

            set
            {
                m_password = value;
            }
        }
    }
    public class BoxPermissions
    {
        public BoxPermissions()
        {

        }
        private Boolean m_can_download = true;
        private Boolean m_can_preview = false;

        public BoxPermissions(Boolean canDownload=true)
        {
            m_can_download = canDownload;
        }
        public bool can_download
        {
            get
            {
                return m_can_download;
            }

            set
            {
                m_can_download = value;
            }
        }

        public bool can_preview
        {
            get
            {
                return m_can_preview;
            }

            set
            {
                m_can_preview = value;
            }
        }
    }

    public class FolderUploadEmail
    {
        public FolderUploadEmail()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access">Valid values are 'open' and 'collaborators'</param>
        public FolderUploadEmail(string access)
        {
            m_access = access;
        }
        private string m_access;
        private string m_email;

        public string access
        {
            get
            {
                return m_access;
            }

            set
            {
                m_access = value;
            }
        }

        public string email
        {
            get
            {
                return m_email;
            }

            set
            {
                m_email = value;
            }
        }
    }

    public class ItemCollection
    {
        public ItemCollection()
        {

        }

        private int m_total_count = 0;
        private Entries[] m_entries;
        private int m_offset = 0;
        private int m_limit = 0;

        public int total_count
        {
            get
            {
                return m_total_count;
            }

            set
            {
                m_total_count = value;
            }
        }

        public Entries[] entries
        {
            get
            {
                return m_entries;
            }

            set
            {
                m_entries = value;
            }
        }

        public int offset
        {
            get
            {
                return m_offset;
            }

            set
            {
                m_offset = value;
            }
        }

        public int limit
        {
            get
            {
                return m_limit;
            }

            set
            {
                m_limit = value;
            }
        }
    }

    public class TagQueryResult
    {
        private string m_type;
        private string m_id;
        private string m_etag;
        private string[] m_tags;

        public string type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public string etag
        {
            get
            {
                return m_etag;
            }

            set
            {
                m_etag = value;
            }
        }

        public string[] tags
        {
            get
            {
                return m_tags;
            }

            set
            {
                m_tags = value;
            }
        }

        public TagQueryResult()
        {

        }
    }

    public class Collaboration
    {
        public Collaboration()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessibleBy">User who is invited to collaboration.
        /// Following parameters should be specified:
        /// type (user/group)
        /// id OR login (boxID or useremail)</param>
        /// <param name="item">The item object that is being shared.</param>
        /// <param name="role">Use BoxEnums.DecodeCollaboratorRole</param>
        public Collaboration(BoxUser accessibleBy, Item_FileFolder item, string role)
        {
            m_accessible_by = accessibleBy;
            m_item = item;
            m_role = role;
        }
        private string m_id;
        private Item_FileFolder m_item;
        private BoxUser m_accessible_by;
        private string m_role;
        private string m_expires_at;
        /*
         can_view_path property has been deactivated, since json.net wants to serialize it as false
         by default.  We can tell json.net to ignore null values when serializing, but Boolean is not
         nullable.

         The problem is this propery can only be used by the OWNER of a folder.  If a non-owner is using
         this folder and this property is include, box api returns an error.
             */
        private Boolean m_can_view_path;
        private string m_status;
        private string m_acknowledged_at;
        private MiniUser m_created_by;
        private string m_created_at;
        private string m_modified_at;

        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public Item_FileFolder item
        {
            get
            {
                return m_item;
            }

            set
            {
                m_item = value;
            }
        }

        public BoxUser accessible_by
        {
            get
            {
                return m_accessible_by;
            }

            set
            {
                m_accessible_by = value;
            }
        }

        public string role
        {
            get
            {
                return m_role;
            }

            set
            {
                m_role = value;
            }
        }

        public string expires_at
        {
            get
            {
                return m_expires_at;
            }

            set
            {
                m_expires_at = value;
            }
        }

        public bool can_view_path
        {
            get
            {
                return m_can_view_path;
            }

            set
            {
                m_can_view_path = value;
            }
        }

        public string status
        {
            get
            {
                return m_status;
            }

            set
            {
                m_status = value;
            }
        }

        public string acknowledged_at
        {
            get
            {
                return m_acknowledged_at;
            }

            set
            {
                m_acknowledged_at = value;
            }
        }

        public MiniUser created_by
        {
            get
            {
                return m_created_by;
            }

            set
            {
                m_created_by = value;
            }
        }

        public string  created_at
        {
            get
            {
                return m_created_at;
            }

            set
            {
                m_created_at = value;
            }
        }

        public string modified_at
        {
            get
            {
                return m_modified_at;
            }

            set
            {
                m_modified_at = value;
            }
        }
    }

    public class MiniUser
    {
        public MiniUser()
        {

        }
        public MiniUser(string id)
        {
            m_id = id;
        }
        private string m_type;
        private string m_id;
        private string m_name;
        private string m_login;

        public string type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        public string login
        {
            get
            {
                return m_login;
            }

            set
            {
                m_login = value;
            }
        }
    }

    public class WebLink
    {
        public WebLink()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentFolderID">ID of the folder that will contain the web link</param>
        /// <param name="url">URL that the Web Link will direct the user to</param>
        /// <param name="name">Web Link name; this is what the user will see in the Box Interface.
        /// Set Null and user will see URL as the 'name'</param>
        /// <param name="description">Web Link description; this appears under the name in the Box Interface</param>
        public WebLink(string parentFolderID, string url, string name, string description)
        {
            m_url = url;
            m_name = name;
            m_description = description;
            m_parent = new Parent(parentFolderID);
        }
        private string m_type = "web_link";
        private string m_id;
        private Parent m_parent;
        private string m_name;
        private string m_url;
        private string m_description;

        public string type
        {
            get
            {
                return m_type;
            }

            set
            {
                m_type = value;
            }
        }

        public string id
        {
            get
            {
                return m_id;
            }

            set
            {
                m_id = value;
            }
        }

        public Parent parent
        {
            get
            {
                return m_parent;
            }

            set
            {
                m_parent = value;
            }
        }

        public string name
        {
            get
            {
                return m_name;
            }

            set
            {
                m_name = value;
            }
        }

        public string url
        {
            get
            {
                return m_url;
            }

            set
            {
                m_url = value;
            }
        }

        public string description
        {
            get
            {
                return m_description;
            }

            set
            {
                m_description = value;
            }
        }
    }

    public class Comment
    {
        
        private string m_type;
        private string m_id = "";
        private Boolean m_is_reply_comment = false;
        private string m_message = "";
        private string m_tagged_message = "";
        private MiniUser m_created_by = null;
        private string created_at = "";
        private Item_FileFolder m_item = null;
        private string m_modified_at = "";

        public string Type { get => m_type; set => m_type = value; }

        /// <summary>
        /// The ID of the comment object
        /// </summary>
        public string Id { get => m_id; set => m_id = value; }

        /// <summary>
        /// Whether or not this comment is a reply to another comment
        /// </summary>
        public bool Is_reply_comment { get => m_is_reply_comment; set => m_is_reply_comment = value; }

        /// <summary>
        /// The comment text that the user typed
        /// </summary>
        public string Message { get => m_message; set => m_message = value; }

        /// <summary>
        /// The string representing the comment text with @mentions included. @mention format is @[id:username] where id is user_id and username is display name. Field is not included by default.
        /// </summary>
        public string Tagged_message { get => m_tagged_message; set => m_tagged_message = value; }

        /// <summary>
        /// A mini user object representing the author of the comment
        /// </summary>
        public MiniUser Created_by { get => m_created_by; set => m_created_by = value; }

        /// <summary>
        /// The time this comment was created
        /// </summary>
        public string Created_at { get => created_at; set => created_at = value; }

        /// <summary>
        /// The object this comment was placed on
        /// </summary>
        public Item_FileFolder Item { get => m_item; set => m_item = value; }

        /// <summary>
        /// The time this comment was last modified
        /// </summary>
        public string Modified_at { get => m_modified_at; set => m_modified_at = value; }


        public Comment()
        {

        }
        /// <summary>
        /// Constructor for comment object
        /// </summary>
        /// <param name="item_type">Object type on which we are making comment.  Use BoxEnum.Object Type-- either a file or a comment </param>
        /// <param name="itemID">ID of the item on which we are commenting.</param>
        /// <param name="message">Text message of comment</param>
        /// <param name="tagged_message">The text of the comment, including @[userid:Username] somewhere in the message to mention the user, which will send them a direct email, letting them know they’ve been mentioned in a comment</param>
        public Comment(BoxEnums.ObjectType item_type, Int64 itemID, string message, string tagged_message)
        {
            m_item = new Item_FileFolder(BoxEnums.DecodeBoxObjectType(item_type), itemID.ToString());
            m_message = message;
            m_tagged_message = tagged_message;
        }
    }


}
