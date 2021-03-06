using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using WebApi.Models;

namespace WebApi.Controllers
{

    public class UserController : ApiController
    {

        [HttpPost]
        [Route("Login")]
        public HttpResponseMessage Login([FromBody]User user)
        {
            //Check validations and return the correct error
            if (!ModelState.IsValid)
                foreach (ModelState modelState in ModelState.Values)
                    foreach (ModelError error in modelState.Errors)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, error);
            User existsUser = GlobalProp.UserList.Find(p => p.UserName == user.UserName);
            if(existsUser!=null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "User name is exists, choose another name.");
            GlobalProp.UserList.Add(user);
            return Request.CreateResponse(HttpStatusCode.OK, "true");
        }

        [HttpGet]
        [Route("GetUsers")]
        public HttpResponseMessage GetUsers()
        {
            lock (GlobalProp.UserList)
            {
                List<User> waitingUsers = GlobalProp.UserList.FindAll(p => p.PartnerUserName == null);
                return Request.CreateResponse(HttpStatusCode.OK, waitingUsers);
            }

        }

        [HttpGet]
        [Route("GetCurrentUser/{userName}")]
        public HttpResponseMessage GetCurrentUser(string userName)
        {
            lock (GlobalProp.UserList)
            {
                User currentUser = GlobalProp.UserList.Find(p => p.UserName == userName);
                return Request.CreateResponse(HttpStatusCode.OK, currentUser);
            }
        }

        [HttpPut]
        [Route("ChoosePatner/{partnerName}")]
        public HttpResponseMessage ChoosePatner(string partnerName, [FromBody]string currentUserName)
        {
            lock (GlobalProp.UserList)
            {
                User currentUser = GlobalProp.UserList.Find(p => p.UserName == currentUserName);
                User partner = GlobalProp.UserList.Find(p => p.UserName == partnerName);
                //If the partner was choosen by another player
                if (partner.PartnerUserName != null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Choose another partner.");
                //If the user was choosen by another player
                if (currentUser.PartnerUserName != null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You have already been choosen to a game.");
                //If the user chose imself
                if (partnerName == currentUserName)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "You can't choose yourself.");
                //Set the partners' partners
                currentUser.PartnerUserName = partnerName;
                partner.PartnerUserName = currentUserName;

                //Create a new game with the two players
                Game newGame = new Game() { Player1 = currentUser, Player2 = partner, CurrentTurn = currentUser.UserName };
                GlobalProp.GameList.Add(newGame);

                return Request.CreateResponse(HttpStatusCode.OK, "true");

            }
        }


    }
}
