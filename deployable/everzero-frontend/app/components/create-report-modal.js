import { useState } from 'react';
import { format } from 'date-fns';
import { FaRegCalendarAlt } from 'react-icons/fa';

export default function CreateReportModal() {
    const [startDate, setStartDate] = useState(format(new Date(new Date().getFullYear(), 0, 1), 'yyyy-MM-dd'));
    const [endDate, setEndDate] = useState(format(new Date(), 'yyyy-MM-dd'));
    const [includeDetails, setIncludeDetails] = useState(false);

    const handleSubmit = (e) => {
        e.preventDefault();
        console.log({ startDate, endDate, includeDetails });
    };

    return (
        <dialog id="create_report_modal" className="modal">
            <div className="modal-box w-fit max-w-5xl">
                <div className="modal-header flex items-center gap-2 mb-4">
                    <FaRegCalendarAlt className="text-primary" />
                    <h3 className="text-lg font-bold">New Report</h3>
                </div>
                <form onSubmit={handleSubmit} className="space-y-4">
                    <h4 className="text-lg font-bold">Report Period</h4>
                    <div className="flex items-center gap-4">
                        <label className="form-control w-full max-w-xs">
                            <span className="label-text">Start Date</span>
                            <div className="relative">
                                <input
                                    type="date"
                                    value={startDate}
                                    onChange={(e) => setStartDate(e.target.value)}
                                    className="input input-bordered w-full"
                                />
                            </div>
                        </label>
                        <label className="form-control w-full max-w-xs">
                            <span className="label-text">End Date</span>
                            <div className="relative">
                                <input
                                    type="date"
                                    value={endDate}
                                    onChange={(e) => setEndDate(e.target.value)}
                                    className="input input-bordered w-full"
                                />
                            </div>
                        </label>
                    </div>
                    <h4 className="text-lg font-bold">Options</h4>
                    <label className="flex items-center gap-2">
                        <input
                            type="checkbox"
                            checked={includeDetails}
                            onChange={(e) => setIncludeDetails(e.target.checked)}
                            className="checkbox"
                        />
                        <span className="label-text">Include per-invoice emission details</span>
                    </label>
                    <div className="modal-action">
                        <button type="button" className="btn" onClick={() => document.getElementById('create_report_modal').close()}>Cancel</button>
                        <button type="submit" className="btn btn-primary">Generate Report</button>
                    </div>
                </form>
            </div>
        </dialog>
    );
}
