using LinqEtSeedEF.Data;
using LinqEtSeedEF.Models;
using LinqEtSeedEF.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqEtSeedEF.Controllers
{
    public class HomeController : Controller
    {
        private readonly LinqEtSeedEFContext _context;

        public HomeController(LinqEtSeedEFContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> Data()
        {
            DataViewModel dataViewModel = new DataViewModel();

            dataViewModel.Clients = await _context.Client.ToListAsync();
            dataViewModel.Commandes = await _context.Commande.ToListAsync();
            dataViewModel.CommandePlats = await _context.CommandePlat.OrderBy(cp => cp.CommandeId).ToListAsync();
            dataViewModel.Plats = await _context.Plat.ToListAsync();
            dataViewModel.Restaurants = await _context.Restaurant.ToListAsync();

            return View(dataViewModel);
        }

        public async Task<IActionResult> Questions()
        {
            QuestionsViewModel questionViewModel = new QuestionsViewModel();

            // ATTENTION: N'enlevez pas ces lignes de code qui semblent peut-�tre inutiles.
            // Nous allons parler de loading au prochain cours et nous allons voir une comment g�rer le loading efficacement.
            // D'ici l�, comprenez simplement que ces lignes load TOUTES les donn�es des tables et les gardent en m�moire pour la dur�e de la requ�te.
            // Normalement, on ne veut PAS travailler de cette mani�re!
            //D�but du code qu'il faut garder
            await _context.Client.ToListAsync();
            await _context.Commande.ToListAsync();
            await _context.CommandePlat.ToListAsync();
            await _context.Plat.ToListAsync();
            await _context.Restaurant.ToListAsync();
            //Fin du code qu'il faut garder

            questionViewModel.PrixPlatLePlusCher = PrixPlatLePlusCher();
            questionViewModel.ValeurTotalDesPlats = ValeurTotalDesPlats();
            questionViewModel.ValeurTotalDesCommandes = ValeurTotalDesCommandes("Patrick Gagn�");
            questionViewModel.PrixCommandeLaPlusCher = PrixCommandeLaPlusCher();

            questionViewModel.VegetarienResto1 = Vegetarien("La graine du p�re George");
            questionViewModel.VegetarienResto2 = Vegetarien("Le Bistro");
            questionViewModel.VegetarienResto3 = Vegetarien("La Belle Province");

            questionViewModel.PlatsVege = PlatsVegeOrdeCroissantDePrix();
            questionViewModel.PlatsLesPlusChers = PlatsLesPlusChersOrdeDecroissantDePrix(3);

            return View(questionViewModel);
        }

        private DecimalViewModel PrixPlatLePlusCher()
        {
            // TODO: �crire la logique pour trouver le prix du plat le plus cher avec une boucle
            var liste = _context.Plat.ToList();
            decimal prix = 0;

            for (int i = 0; i < liste.Count; i++)
            {
                if (liste[i].Prix > prix)
                    prix = liste[i].Prix;
            }

            // TODO: �crire la logique pour trouver le prix du plat le plus cher avec Linq
            // Utilisez Max
            decimal prixLinq = 0;

            prixLinq = _context.Plat.Max(p => p.Prix);

            return new DecimalViewModel("Quel est le prix du plat le plus cher?", prix, prixLinq);
        }

        private DecimalViewModel ValeurTotalDesPlats()
        {
            // TODO: Calculer la valeur totale des plats avec boucle et Linq
            // Utilisez Sum avec Linq

            decimal prixLinq = 0;

            prixLinq = _context.Plat.Sum(p => p.Prix);

            decimal prixBoucle = 0;
            var liste = _context.Plat.ToList();

            foreach (Plat plat in liste)
            {
                prixBoucle += plat.Prix;
            }

            return new DecimalViewModel("Quelle est la valeur totale des plats?", prixLinq, prixBoucle);
        }

        private DecimalViewModel ValeurTotalDesCommandes(string nomClient)
        {
            // TODO: Calculer la valeur totale des commandes du client [nomClient] avec boucle et Linq
            
            // Linq: Utilisez Where et 2 fois Sum
            var listeLinq = _context.Commande.ToList();

            // Attention: c'est plus facile si vous faites un ToList() et faites le linq sur la liste et non pas le DbSet
            // on en parlera au prochain cours
            // Faites votre requ�te Linq sur listeLinq

            var listeCommandeCClient = listeLinq.Where(c => c.Client.Nom == "Patrick Gagné");
            decimal prixLinq = listeCommandeCClient.Sum(c => c.CommandesPlats.Sum(cp => cp.Plat.Prix * cp.Quantite));

            decimal prixBoucle = 0;
            foreach(Commande commande in listeCommandeCClient)
                foreach (CommandePlat commandePlat in commande.CommandesPlats)
                    prixBoucle += commandePlat.Plat.Prix * commandePlat.Quantite;
            

            return new DecimalViewModel("Quelle est la valeur totale des commandes de " + nomClient + "?", prixLinq, prixBoucle);
        }

        private DecimalViewModel PrixCommandeLaPlusCher()
        {
            // TODO: Trouver le c�ut total de la commande la plus ch�re
            
            // Linq: Utilisez Sum et Max
            var listeLinq = _context.Commande.ToList();
            // Attention: c'est plus facile si vous faites un ToList() et faites le linq sur la liste et non pas le DbSet
            // on en parlera au prochain cours
            // Faites votre requ�te Linq sur listeLinq

            decimal prixLinQ = 0;

            prixLinQ = listeLinq.Select(c => c.CommandesPlats.Sum(cp => cp.Plat.Prix * cp.Quantite)).Max();

            decimal prixBoucle = 0;

            foreach(Commande commande in listeLinq)
            {
                decimal prixCommande = 0;

                foreach (CommandePlat commandePlat in commande.CommandesPlats)
                {
                    prixCommande += commandePlat.Plat.Prix * commandePlat.Quantite;
                }

                if (prixCommande > prixBoucle)
                {
                    prixBoucle = prixCommande;
                }
            }

            return new DecimalViewModel("Quel est le prix de la commande la plus ch�re?", prixLinQ, prixBoucle);
        }

        private VegetarienViewModel Vegetarien(string nomDuResto)
        {
            // TODO: Est-ce que le restaurant avec le nom [nomDuRest] a au moins un plat v�g�?
            bool? optionVege = null;

            var restaurant = _context.Restaurant.Where(r => r.Nom == nomDuResto).FirstOrDefault();
            // TODO: Est-ce que le restaurant a UNIQUEMENT des plats v�g�s?
            bool? toutVege = null;

            optionVege = false;
            toutVege = true;

            foreach (Plat plat in restaurant.Plats)
                if (plat.Vegetarien)
                    optionVege = true;
                else
                    toutVege = false;

            // TODO: M�me chose, mais avec Linq
            // Utilisez Where, All et Any
            bool? optionVegeLinq = null;
            bool? toutVegeLinq = null;

            return new VegetarienViewModel("Status v�g�tarien du restaurant : " + nomDuResto, toutVege, toutVegeLinq, optionVege, optionVegeLinq);
        }

        // M�thode pratique pour utiliser List<>.Sort()
        private int ComparerPrix(Plat platA, Plat platB)
        {
            decimal diff = platA.Prix - platB.Prix;
            if (diff > 0)
                return 1;
            if(diff < 0)
                return -1;
            return 0;
        }

        private PlatsViewModel PlatsVegeOrdeCroissantDePrix()
        {
            // Remplir une liste avec les plats v�g�s en ordre croissant de prix
            // Note: Il y a une m�thode ComparerPrix qui est d�j� fournie au dessus
            // Remplir la liste avec une boucle
            List<Plat> plats = new List<Plat>();
            // Obtenir la liste avec Linq
            // Utilisez Where, OrderBy et ToList
            List<Plat> platsLinq = new List<Plat>();

            return new PlatsViewModel("Quels sont les plats v�g�tariens?", plats, platsLinq);
        }

        private PlatsViewModel PlatsLesPlusChersOrdeDecroissantDePrix(int nbPlats)
        {
            // Remplir une liste avec les plats les plus chers en ordre d�croissant
            // La liste doit avoir uniquement [nbPlats] entr�es
            // Utilisez OrderByDescending, Take et ToList
            List<Plat> platsLesPlusChers = new List<Plat>();
            List<Plat> platsLinq = new List<Plat>();
            
            return new PlatsViewModel("Quels sont les plats les plus chers?", platsLesPlusChers, platsLinq);
        }

    }
}
