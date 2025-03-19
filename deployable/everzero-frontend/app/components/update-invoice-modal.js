import { useEffect, useState } from "react";
import { emissionFactorsAtom } from "@/app/atoms/emissionFactorsAtom";
import { useAtom } from "jotai";
import { FaFileInvoiceDollar, FaPlus, FaTrash } from "react-icons/fa";

export default function UpdateInvoiceModal({ invoice, onSubmit, onClose }) {
    const [emissionFactors] = useAtom(emissionFactorsAtom);
    const [formData, setFormData] = useState(null);

    useEffect(() => {
        if (invoice) {
            if (emissionFactors.loaded && emissionFactors.emissionFactors.length > 0) {
                const enrichedLines = invoice.lines.map(line => {
                    const emissionFactor = emissionFactors.emissionFactors.find(ef => ef.id === line.emissionFactorId);
                    return {
                        ...line,
                        category: emissionFactor ? emissionFactor.category : "",
                        subCategories: emissionFactor ? emissionFactor.subCategories : {},
                        emissionFactorUnit: emissionFactor ? emissionFactor.unit : "",
                        emissionFactorId: line.emissionFactorId || ""
                    };
                });

                setFormData({
                    ...invoice,
                    lines: enrichedLines
                });
                
                return;
            }

            setFormData({
                ...invoice,
            });
        }
    }, [invoice, emissionFactors]);

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({ ...prev, [name]: value }));
    };

    const handleLineChange = (index, field, value) => {
        setFormData(prev => {
            const updatedLines = [...prev.lines];
            updatedLines[index] = { ...updatedLines[index], [field]: value };
            return { ...prev, lines: updatedLines };
        });
    };

    const handleEmissionFactorChange = (index, field, value) => {
        setFormData(prev => {
            const updatedLines = [...prev.lines];
            const currentLine = { ...updatedLines[index] };

            if (field === "category") {
                currentLine.category = value;
                currentLine.subCategories = {};
                currentLine.emissionFactorId = "";
                currentLine.emissionFactorUnit = "";

                // Automatically select subcategories if there's only one option
                const categoryFactors = emissionFactors.emissionFactors.filter((ef) => ef.category === value);
                if (categoryFactors[0]) {
                    Object.keys(categoryFactors[0].subCategories).forEach((subCategoryKey) => {
                        const subCategoryOptions = [...new Set(categoryFactors.map((ef) => ef.subCategories[subCategoryKey]))];
                        if (subCategoryOptions.length === 1) {
                            currentLine.subCategories[subCategoryKey] = subCategoryOptions[0];
                        }
                    });

                    // Auto-select unit if all subcategories are filled and there's only one unit option
                    if (Object.keys(currentLine.subCategories).length === Object.keys(categoryFactors[0].subCategories).length) {
                        const unitOptions = getEmissionFactorUnitOptions(value, currentLine.subCategories);
                        if (unitOptions.length === 1) {
                            currentLine.emissionFactorUnit = unitOptions[0].unit;
                            currentLine.emissionFactorId = unitOptions[0].id;
                        }
                    }
                }
            } else if (field.startsWith("subCategory-")) {
                const subCategoryKey = field.split("-")[1];
                currentLine.subCategories = {
                    ...currentLine.subCategories,
                    [subCategoryKey]: value,
                };
                currentLine.emissionFactorId = "";
                currentLine.emissionFactorUnit = "";

                // Auto-select unit if all subcategories are filled
                const categoryFactors = emissionFactors.emissionFactors.find((ef) => ef.category === currentLine.category);
                if (
                    categoryFactors &&
                    Object.keys(currentLine.subCategories).length === Object.keys(categoryFactors.subCategories).length
                ) {
                    const unitOptions = getEmissionFactorUnitOptions(currentLine.category, currentLine.subCategories);
                    if (unitOptions.length === 1) {
                        currentLine.emissionFactorUnit = unitOptions[0].unit;
                        currentLine.emissionFactorId = unitOptions[0].id;
                    }
                }
            } else if (field === "emissionFactorUnit") {
                currentLine.emissionFactorUnit = value;
                const matchingFactor = getEmissionFactorUnitOptions(currentLine.category, currentLine.subCategories)
                    .find(uef => uef.unit === value);
                currentLine.emissionFactorId = matchingFactor ? matchingFactor.id : "";
            }

            updatedLines[index] = currentLine;
            return { ...prev, lines: updatedLines };
        });
    };

    const getSubCategoryOptions = (category, subCategoryKey) => {
        return [
            ...new Set(
                emissionFactors.emissionFactors
                    .filter(ef => ef.category === category)
                    .map(ef => ef.subCategories[subCategoryKey])
            )
        ];
    };

    const getEmissionFactorUnitOptions = (category, subCategories) => {
        return emissionFactors.emissionFactors
            .filter(ef =>
                ef.category === category &&
                Object.entries(subCategories).every(([key, value]) => ef.subCategories[key] === value)
            )
            .map(ef => ({
                unit: ef.unit,
                carbonEmissionKg: ef.carbonEmissionKg,
                id: ef.id
            }));
    };

    const addLine = () => {
        setFormData(prev => ({
            ...prev,
            lines: [
                ...prev.lines,
                {
                    description: "",
                    quantity: 0,
                    unit: "",
                    category: "",
                    subCategories: {},
                    emissionFactorUnit: "",
                    emissionFactorId: ""
                }
            ]
        }));
    };

    const removeLine = (index) => {
        setFormData(prev => ({
            ...prev,
            lines: prev.lines.filter((_, i) => i !== index)
        }));
    };

    const handleSubmit = (e) => {
        e.preventDefault();
        onSubmit(formData);
    };

    if (!formData) return <dialog id="update_invoice_modal" className="manual"></dialog>;

    return (
        <dialog id="update_invoice_modal" className="modal">
            <div className="modal-box w-11/12 max-w-5xl">
                <div className="modal-header flex items-center gap-2 mb-4">
                    <FaFileInvoiceDollar className="text-primary" />
                    <h3 className="text-lg font-bold">Edit Invoice</h3>
                </div>
                <form onSubmit={handleSubmit} className="space-y-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        {/* ... existing header fields ... */}
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Subject</span>
                            </label>
                            <input
                                type="text"
                                name="subject"
                                value={formData.subject || ""}
                                onChange={handleInputChange}
                                className="input input-bordered"
                                required
                            />
                        </div>
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Supplier Name</span>
                            </label>
                            <input
                                type="text"
                                name="supplierName"
                                value={formData.supplierName || ""}
                                onChange={handleInputChange}
                                className="input input-bordered"
                                required
                            />
                        </div>
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Buyer Name</span>
                            </label>
                            <input
                                type="text"
                                name="buyerName"
                                value={formData.buyerName || ""}
                                onChange={handleInputChange}
                                className="input input-bordered"
                                required
                            />
                        </div>
                        <div className="form-control">
                            <label className="label">
                                <span className="label-text">Date</span>
                            </label>
                            <input
                                type="date"
                                name="date"
                                value={
                                    formData.date
                                        ? new Date(formData.date).toISOString().split("T")[0]
                                        : ""
                                }
                                onChange={handleInputChange}
                                className="input input-bordered"
                                required
                            />
                        </div>
                    </div>
                    <div className="divider">Invoice Lines</div>
                    {formData.lines.map((line, index) => (
                        <div key={index} className="space-y-4 p-4 bg-base-200 rounded-lg mb-4">
                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                {/* ... existing line fields ... */}
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text">Description</span>
                                    </label>
                                    <input
                                        type="text"
                                        value={line.description || ""}
                                        onChange={(e) => handleLineChange(index, "description", e.target.value)}
                                        className="input input-bordered"
                                        required
                                    />
                                </div>
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text">Quantity</span>
                                    </label>
                                    <input
                                        type="number"
                                        value={line.quantity || 0}
                                        onChange={(e) => handleLineChange(index, "quantity", e.target.value)}
                                        className="input input-bordered"
                                        required
                                    />
                                </div>
                                <div className="form-control">
                                    <label className="label">
                                        <span className="label-text">Unit</span>
                                    </label>
                                    <input
                                        type="text"
                                        value={line.unit || ""}
                                        onChange={(e) => handleLineChange(index, "unit", e.target.value)}
                                        className="input input-bordered"
                                        required
                                    />
                                </div>
                            </div>
                            {emissionFactors.loaded && emissionFactors.emissionFactors.length > 0 && (
                                <div className="bg-base-100 p-4 rounded-lg">
                                    <h2 className="text-lg font-bold">Emission Factor</h2>
                                    <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                        <div className="form-control">
                                            <label className="label">
                                                <span className="label-text">Category</span>
                                            </label>
                                            <select
                                                className="select select-bordered"
                                                value={line.category || ""}
                                                onChange={(e) => handleEmissionFactorChange(index, "category", e.target.value)}
                                            >
                                                <option value="">Select category</option>
                                                {[...new Set(emissionFactors.emissionFactors.map(ef => ef.category))].map(category => (
                                                    <option key={category} value={category}>
                                                        {category}
                                                    </option>
                                                ))}
                                            </select>
                                        </div>
                                        {line.category && (
                                            <div className="space-y-4">
                                                {Object.keys(
                                                    emissionFactors.emissionFactors.find(ef => ef.category === line.category)?.subCategories || {}
                                                ).map(subCategoryKey => {
                                                    const options = getSubCategoryOptions(line.category, subCategoryKey);
                                                    // Auto-select if only one option
                                                    if (options.length === 1 && !line.subCategories[subCategoryKey]) {
                                                        handleEmissionFactorChange(index, `subCategory-${subCategoryKey}`, options[0]);
                                                    }
                                                    return (
                                                        <div key={subCategoryKey} className="form-control">
                                                            <label className="label">
                                                                <span className="label-text">{subCategoryKey}</span>
                                                            </label>
                                                            <select
                                                                className="select select-bordered"
                                                                value={line.subCategories[subCategoryKey] || ""}
                                                                onChange={(e) => handleEmissionFactorChange(index, `subCategory-${subCategoryKey}`, e.target.value)}
                                                            >
                                                                <option value="">Select {subCategoryKey}</option>
                                                                {options.map(value => (
                                                                    <option key={value} value={value}>
                                                                        {value}
                                                                    </option>
                                                                ))}
                                                            </select>
                                                        </div>
                                                    );
                                                })}
                                            </div>
                                        )}
                                        {line.category &&
                                            Object.keys(line.subCategories).length ===
                                            Object.keys(
                                                emissionFactors.emissionFactors.find(ef => ef.category === line.category)?.subCategories || {}
                                            ).length && (
                                                <div className="form-control">
                                                    <label className="label">
                                                        <span className="label-text">Emission Factor Unit</span>
                                                    </label>
                                                    <select
                                                        className="select select-bordered"
                                                        value={line.emissionFactorUnit || ""}
                                                        onChange={(e) => handleEmissionFactorChange(index, "emissionFactorUnit", e.target.value)}
                                                    >
                                                        <option value="">Select unit</option>
                                                        {getEmissionFactorUnitOptions(line.category, line.subCategories).map(uef => (
                                                            <option key={uef.id} value={uef.unit}>
                                                                {uef.unit} ({uef.carbonEmissionKg} kg CO2e)
                                                            </option>
                                                        ))}
                                                    </select>
                                                </div>
                                            )}
                                        <input
                                            type="hidden"
                                            value={line.emissionFactorId || ""}
                                            onChange={(e) => handleLineChange(index, "emissionFactorId", e.target.value)}
                                        />
                                    </div>
                                </div>
                            )}
                            <div className="form-control">
                                <button
                                    type="button"
                                    className="btn btn-error"
                                    onClick={() => removeLine(index)}
                                >
                                    <FaTrash className="mr-2" /> Remove Line
                                </button>
                            </div>
                        </div>
                    ))}
                    <button type="button" className="btn btn-secondary" onClick={addLine}>
                        <FaPlus className="mr-2" /> Add Line
                    </button>
                    <div className="modal-action justify-between">
                        <button type="button" className="btn" onClick={onClose}>
                            Cancel
                        </button>
                        <button type="submit" className="btn btn-primary">
                            Update Invoice
                        </button>
                    </div>
                </form>
            </div>
        </dialog>
    );
}