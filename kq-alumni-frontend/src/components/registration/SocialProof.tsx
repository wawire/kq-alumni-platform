"use client";

import React from "react";
import { UserGroupIcon, BriefcaseIcon, GlobeAltIcon } from "@heroicons/react/24/outline";

export default function SocialProof() {
  const stats = [
    {
      icon: <UserGroupIcon className="w-6 h-6" />,
      value: "2,500+",
      label: "Alumni Members",
    },
    {
      icon: <GlobeAltIcon className="w-6 h-6" />,
      value: "45+",
      label: "Countries",
    },
    {
      icon: <BriefcaseIcon className="w-6 h-6" />,
      value: "95%",
      label: "Employed",
    },
  ];

  const testimonials = [
    {
      quote: "Joining the KQ Alumni network opened doors I never imagined. The connections and support have been invaluable.",
      author: "Sarah M.",
      role: "Former Cabin Crew, Now HR Director",
    },
    {
      quote: "The mentorship program helped me transition into tech. Best decision I made was staying connected with KQ Alumni.",
      author: "David K.",
      role: "Former Engineer, Now Software Architect",
    },
  ];

  return (
    <div className="space-y-8">
      {/* Stats */}
      <div className="grid grid-cols-3 gap-4">
        {stats.map((stat, index) => (
          <div key={index} className="text-center">
            <div className="inline-flex items-center justify-center w-12 h-12 bg-white/10 rounded-lg mb-2">
              {stat.icon}
            </div>
            <div className="text-2xl font-cabrito font-bold">{stat.value}</div>
            <div className="text-xs text-gray-400 font-roboto">{stat.label}</div>
          </div>
        ))}
      </div>

      {/* Testimonials */}
      <div className="space-y-4">
        {testimonials.map((testimonial, index) => (
          <div
            key={index}
            className="bg-white/5 border border-white/10 rounded-lg p-4"
          >
            <p className="text-sm text-gray-300 italic mb-3 font-roboto">
              "{testimonial.quote}"
            </p>
            <div>
              <p className="text-sm font-cabrito font-semibold">{testimonial.author}</p>
              <p className="text-xs text-gray-400 font-roboto">{testimonial.role}</p>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
