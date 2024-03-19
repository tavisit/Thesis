from astroquery.simbad import Simbad

from data_engineering import GaiaData

star_names = [
    "Sirius B", "Canopus", "Arcturus", "Proxima", "Vega", "Rigel", "Procyon", "Betelgeuse", "Achernar", "Altair",
    "Aldebaran", "Antares", "Spica", "Pollux", "Fomalhaut", "Deneb", "Regulus", "Castor", "Gacrux", "Bellatrix",
    "Elnath", "Miaplacidus", "Alnilam", "Alnair", "Alpheratz", "Regor", "Dubhe", "Mirfak", "Wezen", "Sargas",
    "Kaus Australis", "Avior", "Alkaid", "Menkalinan", "Acrux", "Almach", "Polaris", "Mirach", "Saiph", "Scheat",
    "Diphda", "Mizar", "Algorab", "Algol", "Hamal", "Electra", "Taygeta", "Maia", "Merope", "Alcyone", "Atlas",
    "Pleione", "Zeta Puppis", "Thuban", "Alderamin", "Denebola", "Gienah", "Eltanin", "Rastaban", "Alphecca",
    "Unukalhai", "Schedar", "Deneb Kaitos", "Mirzam", "Aludra", "Suhail", "Mintaka", "Gomeisa", "Enif", "Scheat",
    "Markab", "Alpheratz", "Caph", "Schedar", "Miram", "Algenib", "Algol", "Almach", "Mira", "Tsih", "Celaeno",
    "Sterope", "Merak", "Phecda", "Megrez", "Alioth", "Mizar", "Alcor", "Benetnash", "Sadr", "Errai", "Alderamin",
    "Ankaa", "Fomalhaut", "Vindemiatrix", "Shaula", "Sargas", "Kaus Media", "Nunki", "Rasalhague"
]


def star_name_to_gaia():
    Simbad.reset_votable_fields()
    Simbad.add_votable_fields('ids')
    star_name_to_gaia_dr3 = {}
    for name in star_names:
        try:
            result_table = Simbad.query_object(name)
            ids = result_table['IDS'][0].split('|')
            gaia_id = next((id for id in ids if id.startswith('Gaia DR3 ')), None)
            star_name_to_gaia_dr3[gaia_id] = name
        except Exception as e:
            print(f"Error retrieving data for {name}: {e}")
    return star_name_to_gaia_dr3


def main():
    star_name_to_gaia_dr3 = star_name_to_gaia()
    data = GaiaData(10000)
    data.prepare_data_for_csv(star_name_to_gaia_dr3)
    output_path = "../../Resources/InputManagement/galactic_data.json"
    data.output_data_csv(output_path, ['name', "mass", "temperature", 'position', 'velocity'])


if __name__ == "__main__":
    main()
