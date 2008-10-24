﻿using System;
using System.Data;
using System.Configuration;

using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using CookComputing.XmlRpc;

namespace Engage.Dnn.Publish.Services
{
    public interface IMetaWeblog
    {
        #region MetaWeblog API

        [XmlRpcMethod("metaWeblog.newPost")]
        string AddPost(string blogid, string username, string password, Post post, bool publish);
        
        [XmlRpcMethod("metaWeblog.editPost")]
        bool UpdatePost(string postid, string username, string password, Post post, bool publish);
        
        [XmlRpcMethod("metaWeblog.getPost")]
        Post GetPost(string postid, string username, string password);

        [XmlRpcMethod("metaWeblog.getCategories")]
        CategoryInfo[] GetCategories(string blogid, string username, string password);

        [XmlRpcMethod("metaWeblog.getRecentPosts")]
        Post[] GetRecentPosts(string blogid, string username, string password, int numberOfPosts);
        

        [XmlRpcMethod("metaWeblog.newMediaObject")]
        MediaObjectInfo NewMediaObject(string blogid, string username, string password,
        MediaObject mediaObject);
        #endregion


        #region Blogger API
        [XmlRpcMethod("blogger.deletePost")]
        [return: XmlRpcReturnValue(Description = "Returns true.")]
        bool DeletePost(string key, string postid, string username, string password, bool publish);

        [XmlRpcMethod("blogger.getUsersBlogs")]
        BlogInfo[] GetUsersBlogs(string key, string username, string password);

        [XmlRpcMethod("blogger.getUserInfo")]
        UserInfo GetUserInfo(string key, string username, string password);

        [XmlRpcMethod("blogger.getRecentPosts")]
        BloggerPost[] GetRecentPosts(string key, string blogid,  string username, string password, int numberOfPosts);


        #endregion

        #region Moveable Type API


        [XmlRpcMethod("mt.setPostCategories")]
        bool SetPostCategories(string postid, string username, string password, MTCategory[] cat);
        #endregion



    }

}

