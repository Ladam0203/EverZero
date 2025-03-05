from fastapi import FastAPI

app = FastAPI()

@app.get("/")
async def root():
    return {
        "invoices": [
            {
                "subject": "MCA60043-MOGY-0424",
                "supplierName": "Community Utilities",
                "buyerName": "Mira Mogyin",
                "lines": [
                    {
                        "description": "Heat Consumption",
                        "quantity": 304,
                        "unit": "kWh",
                    },
                    {
                        "description": "Elec Consumption",
                        "quantity": 137.82,
                        "unit": "kWh",
                    }
                ]
            }
        ]
    }
